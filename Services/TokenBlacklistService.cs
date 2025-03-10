namespace CommerceBack.Services
{
	public class TokenBlacklistService
	{
		private readonly HashSet<string> _blacklist = [];

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
