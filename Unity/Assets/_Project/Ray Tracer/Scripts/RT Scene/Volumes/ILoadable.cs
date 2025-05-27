using System;

namespace _Project.Ray_Tracer.Scripts.RT_Scene.Volumes
{
    public interface ILoadable
    {
        static void Load() => throw new NotImplementedException("Static Load() method not implemented in " +
                                                                "loadable class implementing this interface!");
    }
}