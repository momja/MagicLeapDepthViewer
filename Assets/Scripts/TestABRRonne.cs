using System;
using System.Text;
using UnityEngine;
using System.Threading.Tasks;
using System.Net.Http;

using IVLab.ABREngine;
using IVLab.Utilities;
using Newtonsoft.Json.Linq;

public class TestABRRonne : MonoBehaviour
{
    void Start()
    {
        ABREngine.Instance.LoadState<ResourceStateFileLoader>("ronne_test.json");
    }
}