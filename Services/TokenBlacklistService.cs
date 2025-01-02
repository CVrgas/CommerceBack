namespace CommerceBack.Services
{
	public class TokenBlacklistService
	{
		private HashSet<string> _blacklist = new HashSet<string>();

		public bool IsTokenRevoked(string token)
		{
			return _blacklist.Contains(token);
		}

		public void RevokeToken(string token)
		{
			_blacklist.Add(token);
		}
	}
}
