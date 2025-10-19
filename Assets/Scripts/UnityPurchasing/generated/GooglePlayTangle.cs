// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("nvSJe0CeTL3qqMtK5Ggu3OWbscJ8BoX4PykFRIU7hqvPBFuX42NjfVKqzZnojoDzOC4BmyNcVzprqKpxf6nQJWcbpUikutueSYyiqUZDSM6lF5S3pZiTnL8T3RNimJSUlJCVluYsfTyKGAnRtEREkJXd/Ac1CkFBCIi7F0AzQv+V4VK+6aEp2Nc413DZHaved7mbhkqpi0bki27BnbghLXca90Pc+gKgX9LyxNongqIBWUnR9b0Q4oy257JUC+wWFTPu/TvUVXEqCP+9b/yzJZsHt+e+/F5qzm1mv3O50I2NUsDi3vzjnUTghf9LJzpzmj2unjHvH48VatK91XhrjWB9XawXlJqVpReUn5cXlJSVMl5+G3HQ9n2lEI9PbRpJOpeWlJWU");
        private static int[] order = new int[] { 9,4,2,6,9,7,8,7,10,13,12,12,12,13,14 };
        private static int key = 149;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
