using System;
using System.Collections.Generic;
using System.Text;

namespace TelloSdkStandard.actions
{
    public interface IAction
    {
        SdkWrapper.SdkReponses Execute();
    }
}
