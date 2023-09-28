using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZxTree.Core.Models.Security;

internal class SecurityIdentity
{
    public long Id { get; set; }
    public ISet<long> GroupIds { get; set; } = new HashSet<long>();
}
