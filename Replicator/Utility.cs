using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Replicator
{
    public static class Utility
    {
		public static string GetProfilePath(string directory = null)
        {
            var path = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

			if (directory != null)
            {
                path = Path.Combine(path, directory);
            }

            return path;
        }
    }
}
