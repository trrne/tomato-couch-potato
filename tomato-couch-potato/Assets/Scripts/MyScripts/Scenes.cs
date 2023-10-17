using UnityEngine;
using UnityEngine.SceneManagement;
using static UnityEngine.SceneManagement.SceneManager;

namespace trrne.Teeth
{
    public class Scenes
    {
        /// <summary>
        /// nameで指定したシーンをロード
        /// </summary>
        public static void Load(string name)
        {
            LoadScene(name);
        }

        /// <summary>
        /// build indexで指定したシーンをロード
        /// </summary>
        public static void Load(int index)
        {
            LoadScene(index);
        }

        /// <summary>
        /// シーンをリロード
        /// </summary>
        public static void Load()
        {
            LoadScene(active);
        }

        public static void LoadAdditive(string name)
        {
            LoadScene(name, LoadSceneMode.Additive);
        }

        /// <summary>
        /// アクティブなシーンを取得
        /// </summary>
        public static string active => GetActiveScene().name;

        /// <summary>
        /// 全てのシーンの名前
        /// </summary>
        public static string[] names
        {
            get
            {
                var names = new string[sceneCount];
                for (int i = 0; i < sceneCount; i++)
                {
                    names[i] = GetSceneAt(i).name;
                }

                return names.Length switch
                {
                    0 => throw new Karappoyanke("tabun settei sitenai"),
                    _ => names
                };
            }
        }

        public static int Total(ScenesCountingFormat which) => which switch
        {
            ScenesCountingFormat.Unbuilt => sceneCountInBuildSettings,
            ScenesCountingFormat.Built => sceneCount,
            _ => -1
        };

        public static AsyncOperation LoadAsync(string name)
        {
            return LoadSceneAsync(name);
        }

        public static AsyncOperation LoadAsync(string name, LoadSceneMode mode)
        {
            return LoadSceneAsync(name, mode);
        }

        public static AsyncOperation LoadAsync(int index)
        {
            return LoadSceneAsync(index);
        }

        public static AsyncOperation LoadAsync(int index, LoadSceneMode mode)
        {
            return LoadSceneAsync(index, mode);
        }

        public static AsyncOperation UnloadAsync(string name)
        {
            return UnloadSceneAsync(name);
        }

        public static AsyncOperation UnloadAsync(string name, UnloadSceneOptions options)
        {
            return UnloadSceneAsync(name, options);
        }

        public static AsyncOperation UnloadAsync(int index)
        {
            return UnloadSceneAsync(index);
        }

        public static AsyncOperation UnloadAsync(int index, UnloadSceneOptions options)
        {
            return UnloadSceneAsync(index, options);
        }
    }
}