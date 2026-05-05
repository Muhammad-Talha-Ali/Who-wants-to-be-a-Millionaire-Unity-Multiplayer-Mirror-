using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class ApiManager : MonoBehaviour
{
    public static ApiManager Instance;

    public static string URL = "http://quiz.mizsol.com/api/";
    public string Authentication = "login";
    public string FetchMatch = "admin/match";
    public string GetMatchData = "moderator/match/start/";

    public string Username = "admin@admin.com";
    public string Password = "123456789";

    public Team[] teams;

    string token;

    private static readonly HttpClient client = new HttpClient();

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public async Task Authenticate()
    {
        print("Authenticating");
        var values = new Dictionary<string, string>
            {
                { "username", Username },
                { "password", Password }
            };

        var content = new FormUrlEncodedContent(values);
        var response = await client.PostAsync(Path.Combine(URL, Authentication), content);
        var result = await response.Content.ReadAsStringAsync();
        Authenticate data = JsonUtility.FromJson<Authenticate>(result);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", data.data.access_token);
        print(string.Concat("Bearer", data.data.access_token));
    }

    public async Task<Match[]> FetchMatches()
    {
        print("Fetching matches");
        string result = await client.GetStringAsync(Path.Combine(URL, FetchMatch));
        FetchMatches tmp = JsonUtility.FromJson<FetchMatches>(result);
        return tmp.data;
    }

    public async Task<Match> GetCurrentMatchDatas(int id)
    {
        var values = new Dictionary<string, string>();

        var content = new FormUrlEncodedContent(values);
        var response = await client.PostAsync(Path.Combine(URL, GetMatchData, id.ToString()), content);
        var result = await response.Content.ReadAsStringAsync();
        GetMatchData tmp = JsonUtility.FromJson<GetMatchData>(result);

        StartCoroutine(LoadTeamLogos(tmp.data.teams));
        teams = tmp.data.teams;

        return tmp.data;
    }

    public IEnumerator LoadTeamLogos(Team[] teams)
    {
        foreach (Team team in teams)
        {
            print("FOR");
            Texture2D texture2D = new Texture2D(150, 150);
            //using(WWW www = new WWW(team.logo))
            //{
            //    print("Begin WWW");
            //    yield return www;
            //    print("WWW Ended");

            //    www.LoadImageIntoTexture(texture2D);
            //    team.logoSprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(texture2D.width / 2, texture2D.height / 2));
            //    team.logoReady = true;
            //    team.spriteData = texture2D.EncodeToPNG();
            //}


            using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(team.logo))
            {
                yield return www.SendWebRequest();
                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log(www.error);
                }
                else
                {
                    texture2D = ((DownloadHandlerTexture)www.downloadHandler).texture;
                    team.logoSprite = Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(texture2D.width / 2, texture2D.height / 2));
                    team.logoReady = true;
                    byte[] tmp = texture2D.EncodeToPNG();
                    team.spriteData = texture2D.EncodeToPNG();
                }
            }
        }
    }
}
