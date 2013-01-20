using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GilesTrinity.Technicals
{
    /// <summary>
    /// Enumerate all log categories
    /// </summary>
    [Flags]
    public enum LogCategory
    {
        UserInformation = 0,
        ItemValuation = 1,      
        CacheManagement = 2,    
        ScriptRule = 4,
        Configuration = 8,      
        UI = 16,                
        WeaponSwap = 32,        
        Behavior = 64,          
        Performance = 128,
        Targetting = 256,       
        Weight = 512,           
        ProfileTag = 1024,          
        Moving = 2048,          
        GlobalHandler = 4096    
    }
}
