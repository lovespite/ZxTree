using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZxTree.Core.Models.Security;

internal static partial class AcPolicy
{

    public static AcRole QualifyRole(long ownerId, long ownerGroup, SecurityIdentity visitor)
    {
        // owner
        if (ownerId == visitor.Id) return AcRole.Owner;

        // group member
        if (visitor.GroupIds.Contains(ownerGroup)) return AcRole.Group;

        // other
        return AcRole.Other;
    }

    public static bool CheckPermission(long ownerId, long ownerGroup, AcPermission permission, AcOperation operation, SecurityIdentity visitor)
    {
        var role = QualifyRole(ownerId, ownerGroup, visitor);
        return permission.HasPermission(operation, role);
    }
}
