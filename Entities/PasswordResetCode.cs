using System;
using System.Collections.Generic;

namespace CommerceBack.Entities;

public partial class PasswordResetCode
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int Code { get; set; }

    public DateTime ExpiredDate { get; set; }

    public bool IsDisabled { get; set; }
}
