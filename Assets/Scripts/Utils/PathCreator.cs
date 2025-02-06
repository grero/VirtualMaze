using System.Collections;

namespace VirtualMaze.Assets.Scripts.Utils
{
    int Factorial(int f)
    {
        if(f == 0)
            return 1;
        else
            return f * Factorial(f-1); 
    }


    int[] CreatePath(int n, int s, int t)
    {
        int pl;
        pl = Factorial(n)/Factorial(n-2);
        int pl_min = (int)Math.Round(0.9*pl);
        int pl_a = 0;
        Console.WriteLine($"pl: {pl}");
        int[] path = new int[pl];
        bool found = false;

        void dfs(int current, List<int> _path, HashSet<(int,int)> edges)
        {
            if(found)
            {
                return;
            }
            if((current == t)&&(_path.Count >= pl_min))
            {
                _path.CopyTo(path);
                found = true;
                pl_a = _path.Count;
                return;
            }
            for(int j=0;j<n;j++)
            {
                if(j == current)
                {
                    continue;
                }
                (int,int) edge = (current,j);
                if (edges.Contains(edge)==false)
                {
                    edges.Add(edge);
                    _path.Add(j);
                    dfs(j, _path, edges);
                    //Console.WriteLine($"path count: {_path.Count}");
                    edges.Remove(edge);
                    //remove the last item
                    _path.RemoveAt(_path.Count-1);
                }
            }
        }
        HashSet<(int,int)> edges = new HashSet<(int, int)>();
        List<int> _path = new List<int>();
        _path.Add(s);
        dfs(s,_path, edges);
        Array.Resize(ref path, pl_a);
        return path;
    }

}