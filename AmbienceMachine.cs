using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using FishNet;
using DistantLands.Cozy.Data;
using static DistantLands.Cozy.CozyAmbienceModule;
using System.Linq;
using DistantLands.Cozy;

public class AmbienceMachine : NetworkBehaviour
{
    public void ResetCompletely()
    {


        clientbooted = false;
        serverbooted = false;
        currentAmbienceProfile = null;
        m_AmbienceTimer = 0f;

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
        if (!enabled)
            return;

    

        if (Application.isPlaying)
        {
            SetNextAmbience();
            weightedAmbience = new List<WeightedAmbience>() { new WeightedAmbience() { weight = 1, ambienceProfile = currentAmbienceProfile } };
        }
    }



    public AmbienceProfile[] ambienceProfiles = new AmbienceProfile[0];

    public void SetNextAmbience()
    {

        currentAmbienceProfile = WeightedRandom(ambienceProfiles.ToArray());

    }

    public TIMEMASTER timemaster;


    void FixedUpdate()
    {
        if (!InstanceFinder.IsServer)
        {
            return;
        }
        UpdateWeatherWeights();
    }

    public List<AmbienceProfile> allAmbienceProfiles = new List<AmbienceProfile>();
    public List<string> allAmbienceProfileNames = new List<string>();

    public void UpdateWeatherWeights()
    {
        if (Application.isPlaying)
        {
            if (ambienceChangeCheck != currentAmbienceProfile)
            {
                SetAmbience(currentAmbienceProfile);
            }

            if (timemaster)
                m_AmbienceTimer -= Time.deltaTime * timemaster.modifiedTimeSpeed;
            else
                m_AmbienceTimer -= Time.deltaTime * 0.005f;

            if (m_AmbienceTimer <= 0)
            {
                SetNextAmbience();
            }

            foreach (WeightedAmbience i in weightedAmbience)
            {
                i.weight = i.weight * weight;
            }

            weightedAmbience.RemoveAll(x => x.weight == 0 && x.transitioning == false);

        }

        ComputeBiomeWeights();
    }
    public void ComputeBiomeWeights()
    {
        float totalSystemWeight = 0;
        biomes.RemoveAll(x => x == null);

        foreach (CozyAmbienceModule biome in biomes)
        {
            if (biome != this)
            {
                totalSystemWeight += biome.system.targetWeight;
            }
        }

        weight = Mathf.Clamp01(1 - (totalSystemWeight));
        totalSystemWeight += weight;

        foreach (CozyAmbienceModule biome in biomes)
        {
            if (biome.system != this)
                biome.weight = biome.system.targetWeight / (totalSystemWeight == 0 ? 1 : totalSystemWeight);
        }
    }

    public List<CozyAmbienceModule> biomes = new List<CozyAmbienceModule>();

    public void SetAmbience(AmbienceProfile profile)
    {
        currentAmbienceProfile = profile;
        ambienceChangeCheck = currentAmbienceProfile;

        if (weightedAmbience.Find(x => x.ambienceProfile == profile) == null)
            weightedAmbience.Add(new WeightedAmbience() { weight = 0, ambienceProfile = profile, transitioning = true });

        if (currentAmbienceProfile == null)
        {
            Debug.LogError("error null profile");
            return;
        }
        m_AmbienceTimer += Random.Range(currentAmbienceProfile.minTime, currentAmbienceProfile.maxTime);

        if (InstanceFinder.IsServer)
        {
            if (!REFERENCENEXUS.Instance.gameconverter.headless)
            {
                if (!clientbooted)
                {
                   return;
                }
       
            }
            else
            {

       
            }
        }


        if (REFERENCENEXUS.Instance.gameManager._players.Count == 0)
        {

            return;
        }



        if (clientbooted)
        {
            SetAmbienceForClientSERVER(profile.name);

        } else
        {


            SetAmbienceForClientSERVERNORPC(profile.name);
        }


    }

    [ServerRpc(RequireOwnership = false)]
    public void SetAmbienceForClientSERVER(string profName)
    {
        SetAmbienceForObservers(profName);

    }
    
    public void SetAmbienceForClientSERVERNORPC(string profName)
    {
        SetAmbienceForObservers(profName);

    }


    [ObserversRpc(BufferLast = false, ExcludeOwner = false)]
    public void SetAmbienceForObservers(string profName)
    {

        if (!REFERENCENEXUS.Instance.UsingCozySky)
        {
            return;
        }

        SetAmbienceForClient(profName);
    }



    public string GetCurrentAmbience()
    {

        return currentAmbienceProfile.name;

    }
    public void SetAmbienceForClient(string profName)
    {


        AmbienceProfile weatherProfileToUse = REFERENCENEXUS.Instance.networkbroadcaster.GetAmbienceProfileByName(profName);


        if (weatherProfileToUse == null)
        {
            return;
        }


        CozyAmbienceModule cozyAmbienceModule = CozyWeather.instance.moduleHolder.GetComponent<CozyAmbienceModule>();


        if (cozyAmbienceModule == null)
        {
            return;
        }
        cozyAmbienceModule.SetAmbience(weatherProfileToUse);




    }


    
    public void SetAmbience(AmbienceProfile profile, float timeToChange)
    {

        currentAmbienceProfile = profile;
        ambienceChangeCheck = currentAmbienceProfile;

        if (weightedAmbience.Find(x => x.ambienceProfile == profile) == null)
            weightedAmbience.Add(new WeightedAmbience() { weight = 0, ambienceProfile = profile, transitioning = true });

        foreach (WeightedAmbience j in weightedAmbience)
        {

            if (j.ambienceProfile == profile)
                StartCoroutine(j.Transition(1, timeToChange));
            else
                StartCoroutine(j.Transition(0, timeToChange));

        }

        m_AmbienceTimer += Random.Range(currentAmbienceProfile.minTime, currentAmbienceProfile.maxTime);
    }

    public AmbienceProfile currentAmbienceProfile;
    public AmbienceProfile ambienceChangeCheck;
    public float timeToChangeProfiles = 7;
    private float m_AmbienceTimer;
    public float weight;



    public List<WeightedAmbience> weightedAmbience = new List<WeightedAmbience>();


    public WEATHERMACHINE weathermachine;

    public AmbienceProfile WeightedRandom(AmbienceProfile[] profiles)
    {
        AmbienceProfile i = null;
        List<float> floats = new List<float>();
        float totalChance = 0;

        foreach (AmbienceProfile k in profiles)
        {
            float chance;

            if (weathermachine)
                if (k.dontPlayDuring.Contains(weathermachine.currentWeather))
                    chance = 0;
                else
                    chance = k.GetChance2(null);
            else
                chance = k.GetChance(null);;

            floats.Add(chance);
            totalChance += chance;
        }

        if (totalChance == 0)
        {
            i = (AmbienceProfile)Resources.Load("Default Ambience");
            Debug.LogWarning("Could not find a suitable ambience given the current selected profiles and chance effectors. Defaulting to an empty ambience.");
            return i;
        }

        float selection = Random.Range(0, totalChance);

        int m = 0;
        float l = 0;

        while (l <= selection)
        {
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





}
