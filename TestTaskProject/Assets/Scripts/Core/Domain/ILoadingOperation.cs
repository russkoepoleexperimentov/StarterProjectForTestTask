using System;
using System.Threading.Tasks;

public interface ILoadingOperation
{   
    Task Run(Action<float> updateProgress, Action<string> updateContext);
}