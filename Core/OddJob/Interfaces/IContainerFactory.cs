using System;

namespace GlutenFree.OddJob.Interfaces
{
    public interface IContainerFactory
    {
        object CreateInstance(Type typeToCreate);

        /// <summary>
        /// If the DI framework somehow requires explicit releasing of acquired resources, this should be implemented.
        /// </summary>
        /// <param name="usedInstance"></param>
        void Release(object usedInstance);
    }
}