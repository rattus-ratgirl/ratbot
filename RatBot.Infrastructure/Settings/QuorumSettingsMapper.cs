using Riok.Mapperly.Abstractions;

namespace RatBot.Infrastructure.Settings;

[Mapper]
public partial class QuorumSettingsMapper
{
    [MapProperty(nameof(QuorumSettingsEntity.Roles), nameof(QuorumSettings.RoleIds))]
    public partial QuorumSettings Map(QuorumSettingsEntity entity);

    [MapProperty(nameof(QuorumSettings.RoleIds), nameof(QuorumSettingsEntity.Roles))]
    public partial QuorumSettingsEntity Map(QuorumSettings config);

    private static ulong MapRoleToUlong(RoleEntity role) => role.RoleId;

    private static RoleEntity MapUlongToRole(ulong roleId) => new RoleEntity(roleId);
}