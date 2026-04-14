using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace RatBot.Infrastructure.Settings.Quorum;

public sealed class RoleEntityValueConverter() : ValueConverter<ulong, RoleEntity>(
    roleId => new RoleEntity(roleId),
    roleEntity => roleEntity.RoleId)
{
    private static readonly Func<ulong, RoleEntity> ToProviderCompiled =
        new RoleEntityValueConverter().ConvertToProviderExpression.Compile();

    private static readonly Func<RoleEntity, ulong> ToModelCompiled =
        new RoleEntityValueConverter().ConvertFromProviderExpression.Compile();

    public static RoleEntity ToRoleEntity(ulong roleId) => ToProviderCompiled(roleId);

    public static ulong ToRoleId(RoleEntity roleEntity) => ToModelCompiled(roleEntity);
}
