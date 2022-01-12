using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Sol : MonoBehaviour
{

    public GameObject sol;

    public GameObject placas;

    public Text textDotProduct;
    public Text textShadowPercentage;
    public Text textIrradiationPercentage;
    public float latitude = 39;

    public float day = 0;
    public float hour = -12;

    public bool visualize = false;


    private const float axialTilt = 23.439f;

    private float[] results = new float[365 * 24 * 60];
    private int i = 0;

    // Start is called before the first frame update
    void Start()
    {
        if (!visualize)
        {
            for (int i = 0; i < results.Length; i++)
            // for (int i = 0; i < 1; i++)
            {
                hour += 1f / 60f;
                if (hour >= 12)
                {
                    hour -= 24;
                    day += 1;
                }
                SetDayHour(day + (hour / 24), hour);

                results[i] = GetPercentageLight();
            }
            // Save results to file
            string path = Application.dataPath + "/results.txt";
            System.IO.File.WriteAllText(path, string.Join("\n", results));
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (visualize)
        {
            hour += 1f / 60f;
            if (hour >= 12)
            {
                hour -= 24;
                day += 1;
            }
            SetDayHour(day + (hour / 24), hour);
            if (i < results.Length)
            // if (false)
            {
                results[i] = GetPercentageLight();
                i++;
            }
            else
            {
                string path = Application.dataPath + "/results.txt";
                System.IO.File.WriteAllText(path, string.Join("\n", results));
                gameObject.SetActive(false);
            }
        }
    }

    void SetDayHour(float day, float hour)
    {
        // if(day > 365 || day < 0){
        //     throw Exception();
        // }
        //float angle = (90 - latitude) + axialTilt * Mathf.Sin(day / 365);
        // print(day + " - " + hour);
        float anglex = GetZenith(day, hour) * Mathf.Rad2Deg;
        float angley = GetAzimuth(day, hour) * Mathf.Rad2Deg;
        float angleybef = GetAzimuth(day, hour - 0.1f) * Mathf.Rad2Deg;

        if (angleybef < angley)
        {
            print("Inverting");
            angley = 180 - angley;
        }

        print("Zen: " + anglex);
        print("Azi: " + angley);

        Vector3 rot = sol.transform.eulerAngles;
        rot.x = 90 - anglex;
        rot.y = -angley;
        sol.transform.eulerAngles = rot;

        // sol.gameObject.SetActive(90 - anglex > 0);

    }

    float GetDeclination(float day)
    {
        return -axialTilt * Mathf.Deg2Rad * Mathf.Cos((2 * Mathf.PI / 365) * (day + 10));
    }

    float GetHourAngle(float hour)
    {
        return hour * 2 * Mathf.PI / 24;
    }

    float GetZenith(float day, float hour)
    {
        float hourAngle = GetHourAngle(hour);
        float declination = GetDeclination(day);
        return Mathf.Acos(
            Mathf.Sin(latitude * Mathf.Deg2Rad) * Mathf.Sin(declination) +
            Mathf.Cos(latitude * Mathf.Deg2Rad) * Mathf.Cos(declination) *
            Mathf.Cos(hourAngle)
        );
    }

    float GetAzimuth(float day, float hour)
    {
        float hourAngle = GetHourAngle(hour);
        float declination = GetDeclination(day);
        float zenith = GetZenith(day, hour);
        print(hourAngle * Mathf.Rad2Deg + "_" + declination * Mathf.Rad2Deg + "_" + zenith * Mathf.Rad2Deg);
        float azi = Mathf.Asin(
            (-Mathf.Sin(hourAngle) * Mathf.Cos(declination)) / (Mathf.Sin(zenith))
        );
        print(azi * Mathf.Rad2Deg);

        return azi;
    }

    float GetPercentageLight()
    {
        Vector3 direction = -sol.transform.forward;

        Vector3 pRight = placas.transform.right * placas.transform.localScale.x;
        Vector3 pUp = placas.transform.up * placas.transform.localScale.y;
        int inShadow = 0;

        int side = 15;

        float dotProduct = Vector3.Dot(-direction.normalized, placas.transform.forward.normalized);
        for (int i = 0; i < side; i++)
        {
            for (int j = 0; j < side; j++)
            {
                Vector3 origin = placas.transform.position + pRight * ((i - (side / 2)) / (side - 1f)) + pUp * ((j - (side / 2)) / (side - 1f));
                RaycastHit hit;
                // Debug.DrawRay(origin, direction, Color.red, 0.05f, false);
                if (Physics.Raycast(origin, direction, out hit))
                {
                    if (visualize)
                        Debug.DrawRay(origin, direction * hit.distance, Color.red, 0.05f, true);
                    inShadow++;
                }
                else
                {
                    if (visualize)
                        if (dotProduct < 0)
                        {
                            Debug.DrawRay(origin, direction * 20, Color.red, 0.05f, true);
                        }
                        else
                        {
                            Debug.DrawRay(origin, direction * 20, Color.green, 0.05f, true);
                        }
                }
            }
        }

        if (dotProduct < 0)
        {
            dotProduct = 0;
            inShadow = side * side;
        }

        float result = (1f - (inShadow / (1f * side * side))) * dotProduct;

        if (visualize)
        {
            textDotProduct.text = "" + result;
            textShadowPercentage.text = "" + (1f - (inShadow / (1f * side * side)));
            textIrradiationPercentage.text = "" + Vector3.Dot(-direction.normalized, placas.transform.forward.normalized);
        }
        return result;
    }
}
