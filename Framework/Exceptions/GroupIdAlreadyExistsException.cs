using CodeStack.Dev.Sw.AddIn.Attributes;
using System;

namespace CodeStack.Dev.Sw.AddIn.Exceptions
{
    public class GroupIdAlreadyExistsException : Exception
    {
        public GroupIdAlreadyExistsException(int groupId) 
            : base($"Group id {groupId} already exists. Make sure that all group enumerators decorated with {typeof(CommandGroupInfoAttribute)} have unique values for id")
        {
        }
    }
}
