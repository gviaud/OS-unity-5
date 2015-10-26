using UnityEngine;
using System.Collections;

public static class ExtraApi
{
	public static string appName = "third_party";
	public static string thirdPartyKey = "third_party_key";
	public static string extraLogin = "user_login";
	public static string extraMdp = "user_password";
	public static string returnFormat = "format_retour";	
	public static string authValid = "is_auth";
	public static string privateKey = "user_private_key";
	public static string shops = "user_magasins";
	public static string like = "like";
	public static string clientsList = "clients_list";
	public static string idClient = "client_id";
	public static string idShop = "magasin";
	public static string fileName = "file_name";
	public static string fileTransferred = "file_transferred";
	
	#region URL's
	public static string url_Main = "http://v4.extrabat.com";//replaced by "https://myextrabat.com"
	public static string url_AuthExt = "/api/auth";
	public static string url_srchClientExt = "/api/seach_clients";
	public static string url_fileDropExt = "/api/file_from_ftp_to_client";
	public static string url_FTP = "...";
	#endregion
	
}
