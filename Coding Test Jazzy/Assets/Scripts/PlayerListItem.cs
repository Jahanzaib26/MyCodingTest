using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Steamworks;

public class PlayerListItem : MonoBehaviour
{
    public string PlayerName;
    public int ConnectionID;
    public ulong PlayerSteamID;
    private bool AvatarRecieved;



    //  UI Elements
    public Text PlayerNameText;
    public RawImage PlayerIcon;
    public Text PlayerReadyText;
    public bool Ready;

    protected Callback<AvatarImageLoaded_t> ImageLoaded;



    public void ChangeReadyStatus()
    {
        if (Ready)
        {
            PlayerReadyText.text = "Ready";
            PlayerReadyText.color = Color.green;
        }
        else
        {
            PlayerReadyText.text = "UnReady";
            PlayerReadyText.color = Color.red;
        }

    }
    private void Start()
    {
        ImageLoaded = Callback<AvatarImageLoaded_t>.Create(OnImageLoaded);
    }



    public void SetPlayerVslues()
    {
        if (!this || !gameObject)
            return;
        if (PlayerNameText == null)
            return;

        PlayerNameText.text = PlayerName;
        ChangeReadyStatus();
        if (!AvatarRecieved)
        {

            GetPlayerIcon();
        }
    }

    private void GetPlayerIcon()
    {
        int ImageID = SteamFriends.GetLargeFriendAvatar((CSteamID)PlayerSteamID);

        if (ImageID == -1)
        {
            return; // wait until OnImageLoaded fires
        }
        else
        {
            PlayerIcon.texture = GetSteamImageAsTexture(ImageID);
        }
    }
    private Texture2D GetSteamImageAsTexture(int iImage)
    {
        Texture2D texture = null;

            bool isvalid= SteamUtils.GetImageSize(iImage, out uint width, out uint height);

            if (isvalid) {

                byte[] image = new byte[width * height * 4];
                isvalid = SteamUtils.GetImageRGBA(iImage, image, (int)(width*height*4));

                if (isvalid) { 
                
                    texture = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false, true);
                    texture.LoadRawTextureData(image);
                    texture.Apply();
                
                }
            
            }
            AvatarRecieved= true;
            return texture;

    }



    private void OnImageLoaded(AvatarImageLoaded_t callback)
    {
        if(callback.m_steamID.m_SteamID == PlayerSteamID)
        {
            PlayerIcon.texture = GetSteamImageAsTexture(callback.m_iImage); 
        }
        else  // another player
        {
            return;
        }
    }

}
