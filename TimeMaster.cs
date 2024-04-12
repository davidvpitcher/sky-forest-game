    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using FishNet.Object;
    using FishNet;
    using DistantLands.Cozy;
    using DistantLands.Cozy.Data;
    using FishNet.Connection;

    public class TimeMaster : NetworkBehaviour
    {


        public CozyTransitModule transit;

        private int daysPerYear = 48;

        public CozyDateOverride overrideDate;
        [MeridiemTimeAttriute]
        [SerializeField]
        private float m_DayPercentage = 0.5f;
        [Range(0, 1)]
        public float yearPercentage = 0;
        private float modifiedDayPercentage;
        public float timeSpeed;
    private bool isNight = false;

    public void MakeNearNight()
    {

        float nearNightTime = 0.825f;

        m_DayPercentage = nearNightTime;
        currentTime = m_DayPercentage;
        isNight = false;
        SyncWeather(true);


    }
    public void MakeNearMorning()
    {

        float nearMorningTime = 0.245f; 

        m_DayPercentage = nearMorningTime;
        currentTime = m_DayPercentage;
        isNight = true;
        SyncWeather(true);



    }
    public bool IsItNighttime()
    {
        return isNight;
    }
    public MeridiemTime currentTime
        {
            get { return m_DayPercentage; }
            set { m_DayPercentage = value; }

        }

        public int currentDay;
        public int currentYear;

        public void ResetComponent()
        {

            m_DayPercentage = 0.5f;
            yearPercentage = 0f;
            modifiedDayPercentage = 0f;
            timeSpeed = 0f;
            currentDay = 0;
            currentYear = 0;

            daysPerYear = 48;
            currentTime = m_DayPercentage;



        }
    
        public void ResetComponentToNight()
        {

            m_DayPercentage = 0.9f;
            yearPercentage = 0f;
            modifiedDayPercentage = 0f;
            timeSpeed = 0f;
            currentDay = 0;
            currentYear = 0;

            daysPerYear = 48;
            currentTime = m_DayPercentage;


        }

        public void ResetTime()
        {


            ResetTimeSERVER(this);
        }
        public void MakeNight()
        {


            MakeNightSERVER(this);
        }

        [ServerRpc(RequireOwnership = false)]
        public void ResetTimeSERVER(TimeMaster timemaster)
        {

            ResetComponent();
            timemaster.SyncWeather(true);

        }
    
        [ServerRpc(RequireOwnership = false)]
        public void MakeNightSERVER(TimeMaster timemaster)
        {

            ResetComponentToNight();




            timemaster.SyncWeather(true);

        }

        public void SyncWeather(bool reset)
        {

            SyncWeatherFORALL(this, m_DayPercentage, currentDay, currentYear, reset);

        }

        [ObserversRpc(BufferLast = false, ExcludeOwner = false)]
        public void SyncWeatherFORALL(TimeMaster timemaster, float dayPercentage, int currentday, int currentyear, bool reset)
        {

            if (!reset)
            {
                if (InstanceFinder.IsServer)
                {
                    return;
                }
            } else
            {
                if (REFERENCENEXUS.Instance.gameconverter.headless)
                {
                    return;
                }

            }

            timemaster.m_DayPercentage = dayPercentage;
            timemaster.currentDay = currentday;
            timemaster.currentYear = currentyear;
            timemaster.currentTime = dayPercentage;
            if (REFERENCENEXUS.Instance.weatherSystem != null)
            {

            CozyWeather cozyWeather = REFERENCENEXUS.Instance.weatherSystem.GetComponent<CozyWeather>();
            cozyWeather.timeModule.modifiedDayPercentage = dayPercentage;
            cozyWeather.timeModule.currentDay = currentday;
            cozyWeather.timeModule.currentYear = currentyear;
            cozyWeather.timeModule.currentTime = dayPercentage;

            }


        }


        public void RequestWeatherSync(NetworkConnection conn)
        {

            networkbroadcaster.LearnDataFromTimeMaster(conn, m_DayPercentage, currentDay, currentYear, IsItNighttime());


        }
        public NETWORKBROADCASTER networkbroadcaster;

        void FixedUpdate()
        {


            ManageTime();

            yearPercentage = GetCurrentYearPercentage();
      
        }

 
        public float GetCurrentYearPercentage()
        {

            if (overrideDate)
                return overrideDate.GetCurrentYearPercentage();

            float dat = DayAndTime();
            return dat / daysPerYear;
        }
        public float GetCurrentYearPercentage(float inTIme)
        {
            if (overrideDate)
                return overrideDate.GetCurrentYearPercentage(inTIme);

            float dat = DayAndTime() + inTIme;
            return dat / daysPerYear;
        }
  
        public float DayAndTime()
        {
            if (overrideDate)
                return overrideDate.DayAndTime();

            return currentDay + m_DayPercentage;

        }

    public void ManageTime()
    {
        if (Application.isPlaying)
        {
            float previousDayPercentage = m_DayPercentage;
            m_DayPercentage += modifiedTimeSpeed * Time.deltaTime;

            // Check if transitioning from day to night (20:00)
            if (!isNight && previousDayPercentage < 5f / 6f && m_DayPercentage >= 5f / 6f)
            {
                OnNightStarted();
            }

        

            if (isNight && previousDayPercentage < 6f / 24f && m_DayPercentage >= 6f / 24f)
            {
                OnNightEnded();
            }
        }

        ConstrainTime();
    }

    private void OnNightStarted()
    {
        isNight = true;

        if (REFERENCENEXUS.Instance.DontAlwaysRunChests)
        {
            if (REFERENCENEXUS.Instance.EmptyGame())
            {
                return;
            }
        }

        if (REFERENCENEXUS.Instance.lootdropsystem != null)
        {

            REFERENCENEXUS.Instance.lootdropsystem.OnNightStarted();
        }

        if (!REFERENCENEXUS.Instance.EmptyGame())
        {

            REFERENCENEXUS.Instance.networkbroadcaster.BroadCastNightChanged(isNight);
       
            return;
        }
    }

    private void OnNightEnded()
    {
        isNight = false;

        REFERENCENEXUS.Instance.lootdropsystem.OnNightEnded();


        if (!REFERENCENEXUS.Instance.EmptyGame())
        {

            REFERENCENEXUS.Instance.networkbroadcaster.BroadCastNightChanged(isNight);
 
            return;
        }


    }


    private void ConstrainTime()
        {
            if (m_DayPercentage >= 1)
            {
                m_DayPercentage -= 1;
                ChangeDay(1);
       
            }

            if (m_DayPercentage < 0)
            {
                m_DayPercentage += 1;
                ChangeDay(-1);
            }
        }


        private void ChangeDay(int change)
        {

            if (overrideDate)
            {
                overrideDate.ChangeDay(change);
                return;
            }

            currentDay += change;

            if (currentDay >= daysPerYear)
            {
                currentDay -= daysPerYear;
                currentYear++;
        
            }

            if (currentDay < 0)
            {
                currentDay += daysPerYear;
                currentYear--;
            }
        }



        public float modifiedTimeSpeed
        {
            get
            {
                return timeMovementSpeed * timeSpeedMultiplier.Evaluate(m_DayPercentage) / 1440;
            }
        }

        public float timeMovementSpeed = 1f;


        public AnimationCurve timeSpeedMultiplier;
        public override void OnStartClient()
        {
            base.OnStartClient();




            if (!InstanceFinder.IsServer)
            {
                this.enabled = false;
                return;
            }




        }











    }
