using DistantLands.Cozy.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DistantLands.Cozy;
using static DistantLands.Cozy.CozyEcosystem;
using System.Linq;
using FishNet;
using FishNet.Object;
using static DistantLands.Cozy.ChanceEffector;

public class WeatherMachine : NetworkBehaviour
{

    public void ResetCompletely()
    {

        clientbooted = false;
        serverbooted = false;
        currentWeather = null;
        weatherTimer = 0f;



    }

    public void SetWeatherByName(string newWeather)
    {
        currentWeather = GetServerWeatherProfileByName(newWeather);

        SetWeather(currentWeather);
    }


    public WeatherProfile GetServerWeatherProfileByName(string profname)
    {

        WeatherProfile weatherProfileToUse = null;



        foreach (WeatherProfile weatherProfile in weatherProfiles)
        {

            if (weatherProfile.name == profname)
            {
                weatherProfileToUse = weatherProfile;
                break;
            }
        }


        return weatherProfileToUse;

    }


    public bool clientbooted = false;
    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!IsServer)
        {
            this.enabled = false;
        }

        clientbooted = true;
    }

    public bool serverbooted = false;
    public override void OnStartServer()
    {
        base.OnStartServer();

        serverbooted = true;
    }
    public void FixedUpdate()
    {
        if (!InstanceFinder.IsServer)
        {
            return;
        }

        UpdateBiomeModule();
     



    }

    public TIMEMASTER timemaster;

    public void UpdateBiomeModule()
    {
        UpdateEcosystem();

    }
    public float weatherTransitionTime = 15;
    public List<WeatherPattern> currentForecast = new List<WeatherPattern>();

    public List<WeatherProfile> allWeatherProfiles = new List<WeatherProfile>();

    [ServerRpc(RequireOwnership = false)]
    public void SetWeatherForClientSERVER(string profName, float transitionDuration)
    {

        SetWeatherForObservers(profName, transitionDuration);

    }

    public void SetWeatherForClientSERVERNORPC(string profName, float transitionDuration)
    {


     SetWeatherForObservers(profName, transitionDuration);

    }

    [ObserversRpc(BufferLast = false, ExcludeOwner = false)]
    public void SetWeatherForObservers(string profName, float transitionDuration)
    {

        if (!REFERENCENEXUS.Instance.UsingCozySky)
        {
            return;
        }


       REFERENCENEXUS.Instance.networkbroadcaster.SetWeatherForClient(profName, transitionDuration);
    }

    public string GetCurrentWeather()
    {

        return currentWeather.name;

    }
    public List<WeatherProfile> weatherProfiles = new List<WeatherProfile>();

        public List<string> weatherProfileNames= new List<string>();



    public float weatherTimer { get; private set; }
    public void SetNextWeather()
    {

        SetupWeatherForecast();
        if (currentForecast.Count == 0)
            ForecastNewWeather();

        SetWeather(currentForecast[0].profile);
        weatherTimer += currentForecast[0].duration;

        currentForecast.RemoveAt(0);
        ForecastNewWeather();

    }
    public void SetWeather(WeatherProfile prof)
    {

        SetWeather(prof, weatherTransitionTime);

    }
    [WeatherRelation]
    public List<WeatherRelation> weightedWeatherProfiles = new List<WeatherRelation>();


    public WeatherProfile weatherChangeCheck;
    public WeatherProfile currentWeather;


    public void SetWeather(WeatherProfile prof, float transitionTime)
    {
        if (!serverbooted)
        {
            Debug.LogError("Set weather but not booted");
        }
    
        currentWeather = prof;
        weatherChangeCheck = currentWeather;

        if (weightedWeatherProfiles.Find(x => x.profile == prof) == null)
            weightedWeatherProfiles.Add(new WeatherRelation() { profile = prof, weight = 0, transitioning = true });

    
        if (InstanceFinder.IsServer)
        {
            if (!REFERENCENEXUS.Instance.gameconverter.headless)
            {
                if (!clientbooted)
                {
                  //  Debug.Log("Unable to set client weather for non-headless server");
                    return;
                }
         
            }
            else
            {

             //   Debug.Log("Client not booted but attempting to set weather");

            }
        }
        

        if (REFERENCENEXUS.Instance.gameManager._players.Count == 0)
        {

            return;
        }


        if (clientbooted)
        {
            SetWeatherForClientSERVER(prof.name, transitionTime);
        } else
        {

            SetWeatherForClientSERVERNORPC(prof.name, transitionTime);
        }

    }

    public void ForecastNewWeather()
    {

        WeatherProfile weatherProfile;

        if (currentForecast.Count > 0)
            weatherProfile = PickRandomWeather(GetNextWeatherArray(forecastProfile.profilesToForecast.ToArray(), currentForecast.Last().profile.forecastNext, currentForecast.Last().profile.forecastModifierMethod));
        else
            weatherProfile = PickRandomWeather(forecastProfile.profilesToForecast.ToArray());

        ForecastNewWeather(weatherProfile, Random.Range(weatherProfile.minWeatherTime, weatherProfile.maxWeatherTime));

    }
    public ForecastProfile forecastProfile;
    public void ForecastNewWeather(WeatherProfile weatherProfile, float duration)
    {

        WeatherPattern i = new WeatherPattern
        {
            profile = weatherProfile
        };

            i.startTime = timemaster.currentTime + weatherTimer;

            foreach (WeatherPattern j in currentForecast)
                i.startTime += j.duration;

            i.endTime = (i.startTime + duration) % 1;
            i.startTime %= 1;
   

        currentForecast.Add(i);

    }



    public void ForecastNewWeather(WeatherProfile weatherProfile)
    {

        ForecastNewWeather(weatherProfile, Random.Range(weatherProfile.minWeatherTime, weatherProfile.maxWeatherTime));

    }
    WeatherProfile[] GetNextWeatherArray(WeatherProfile[] total, WeatherProfile[] exception, WeatherProfile.ForecastModifierMethod modifierMethod)
    {

        switch (modifierMethod)
        {

            case (WeatherProfile.ForecastModifierMethod.DontForecastNext):
                return SubtractiveArray(total, exception);
            case (WeatherProfile.ForecastModifierMethod.forecastNext):
                return IntersectionArray(total, exception);
            default:
                return total;

        }

    }


    WeatherProfile[] SubtractiveArray(WeatherProfile[] total, WeatherProfile[] subtraction)
    {

        return total.ToList().Except(subtraction.ToList()).ToArray();

    }

    WeatherProfile[] IntersectionArray(WeatherProfile[] total, WeatherProfile[] intersection)
    {

        return intersection.ToList().Except(intersection.ToList().Except(total.ToList())).ToArray();

    }
    [ChanceEffector]
    public List<ChanceEffector> chances = new List<ChanceEffector>();

  

    public CozyClimateModule climateModule;


    public float GetChance(float inTime)
    {
        float i = likelihood;
        foreach (ChanceEffector j in chances)
        {
            i *= j.GetChance2(null);
        }
        return i > 0 ? i : 0;
    }

    [Range(0, 2)]
    public float likelihood = 1;


    WeatherProfile PickRandomWeather(WeatherProfile[] profiles)
    {

        if (profiles.Count() == 0)
            profiles = forecastProfile.profilesToForecast.ToArray();

        WeatherProfile i = null;
        List<float> floats = new List<float>();
        float totalChance = 0;
        float weatherStartTime = 0;

        foreach (WeatherPattern k in currentForecast)
            weatherStartTime += k.endTime - k.startTime;

 

        foreach (WeatherProfile k in profiles)
        {
            float chance = k.GetChance2(null, weatherStartTime);
            floats.Add(chance);
            totalChance += chance;
        }

        float selection = Random.Range(0, totalChance);

        int m = 0;
        float l = 0;

        while (l <= selection)
        {
            if (m >= floats.Count)
            {
                i = profiles[profiles.Length - 1];
                break;
            }

            if (selection >= l && selection < l + floats[m])
            {
                i = profiles[m];
                break;
            }
            l += floats[m];
            m++;

        }

        if (!i)
        {
            i = profiles[0];
        }

        return i;
    }

    public void SetupWeatherForecast()
    {
        while (currentForecast.Count < forecastProfile.forecastLength)
        {
            ForecastNewWeather();
        }
    }

    public void UpdateEcosystem()
    {
      
    
        if (Application.isPlaying)
        {
            // current weather mode was forecast

        
                    weatherTimer -= Time.deltaTime * timemaster.modifiedTimeSpeed;
            

                while (weatherTimer <= 0)
                    SetNextWeather();

         

            if (weatherChangeCheck != currentWeather)
            {
                SetWeather(currentWeather, weatherTransitionTime);
            }

            weightedWeatherProfiles.RemoveAll(x => x.weight == 0 && x.transitioning == false);
        }
        else
        {
     
            if (weatherChangeCheck != currentWeather)
            {
                if (weatherChangeCheck)
                    weatherChangeCheck.SetWeatherWeight(0);

                weatherChangeCheck = currentWeather;
            }
        }


        ClampEcosystem();
    }

    public void ClampEcosystem()
    {
   
        float j = 0;

        foreach (WeatherRelation i in weightedWeatherProfiles) j += i.weight;

        if (j == 0)
            j = 1;

        foreach (WeatherRelation i in weightedWeatherProfiles) i.weight /= j;
     
    }



}
