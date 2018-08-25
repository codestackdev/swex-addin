//**********************
//SwEx.AddIn - development tools for SOLIDWORKS add-ins
//Copyright(C) 2018 www.codestack.net
//License: https://github.com/codestack-net-dev/sw-dev-tools-addin/blob/master/LICENSE
//Product URL: https://www.codestack.net/labs/solidworks/swex/add-in/
//**********************

using CodeStack.SwEx.AddIn.Attributes;
using System;

namespace CodeStack.SwEx.AddIn.Exceptions
{
    /// <summary>
    /// Exception indicates that specified group user id is already used
    /// </summary>
    /// <remarks>This might happen when <see cref="CommandGroupInfoAttribute"/> explicitly specifies duplicate user ids.
    /// This can also happen that not all commands have this attribute assigned explicitly.
    /// In this case framework is attempting to generate next user id which might be already taken by explicit declaration</remarks>
    public class GroupIdAlreadyExistsException : Exception
    {
        internal GroupIdAlreadyExistsException(int groupId) 
            : base($"Group id {groupId} already exists. Make sure that all group enumerators decorated with {typeof(CommandGroupInfoAttribute)} have unique values for id")
        {
        }
    }
}
