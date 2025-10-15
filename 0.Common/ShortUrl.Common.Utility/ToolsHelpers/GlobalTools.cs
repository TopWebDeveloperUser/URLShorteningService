namespace ShortUrl.Common.Utility.ToolsHelpers
{
    public static class GlobalTools
    {
        public static void DeleteDirectory(string folderPath)
        {
            if (!Directory.Exists(folderPath))
                return;

            DeleteFolderContent(folderPath);
            Directory.Delete(folderPath);
        }

        public static void DeleteFolderContent(string folderPath)
        {
            var files = Directory.GetFiles(folderPath);
            foreach (var f in files)
                File.Delete(f);

            var folders = Directory.GetDirectories(folderPath);
            foreach (var folder in folders)
                DeleteDirectory(folder);
        }
        public static List<Dictionary<string, object>> ListObjectToDictionay(IEnumerable<object> data)
        {
            if (data == null) return null;
            if (!data.Any()) return new List<Dictionary<string, object>>();

            var type = data.First().GetType();
            var props = type.GetProperties();

            var list = new List<Dictionary<string, object>>();
            foreach (var item in data)
            {
                var dic = new Dictionary<string, object>();
                foreach (var p in props)
                    dic.Add(p.Name, p.GetValue(item));

                list.Add(dic);
            }
            return list;
        }
    }
}
