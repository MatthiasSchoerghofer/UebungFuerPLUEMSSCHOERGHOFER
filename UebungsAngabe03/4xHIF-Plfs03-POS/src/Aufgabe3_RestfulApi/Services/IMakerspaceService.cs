using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Aufgabe3_RestfulApi.Cmds;
using Aufgabe3_RestfulApi.Dtos;
using Aufgabe3_RestfulApi.Model;

namespace Aufgabe3_RestfulApi.Services;

public interface IMakerspaceService
{
    public Task<PagedResultDto<DeviceListDto>> GetDevicePaged(DeviceType? deviceType, DeviceStatus? deviceStatus, string? name, int page, int pageSize, CancellationToken ct);
    public Task<DeviceDetailDto?> GetDeviceDetail(int id, CancellationToken ct);
    public Task<DeviceDetailDto> CreateDevice(CreateDeviceCmd cmd, CancellationToken ct);
    public Task<DeviceDetailDto> UpdateDeviceStatus(int id, UpdateDeviceStatusCmd cmd, CancellationToken ct);
    public Task<DeviceDetailDto> DeleteDevice(int id, CancellationToken ct);
    public Task<ReservationDto> CreateReservation(CreateReservationCmd cmd, CancellationToken ct);
    public Task<ReservationDto> PutReservation(int id,CompleteReservationCmd cmd ,CancellationToken ct);
    public Task<MemberReservationDto> GetMemberReservation(int id,ReservationStatus? status , CancellationToken ct); 
    public Task<IEnumerable<MaintenanceDto>> GetMaintenance(DeviceType? type,CancellationToken ct);
    public Task<MaintenanceDto> CreateMaintenanceTicket(CreateMaintenanceTicketCmd cmd, CancellationToken ct);
    public Task<IEnumerable<DeviceTypeStatsDto>> GetDeviceTypInfo(CancellationToken ct);
}
