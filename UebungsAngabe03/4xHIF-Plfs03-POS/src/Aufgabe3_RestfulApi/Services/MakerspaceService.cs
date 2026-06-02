using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Aufgabe3_RestfulApi.Cmds;
using Aufgabe3_RestfulApi.Dtos;
using Aufgabe3_RestfulApi.Infrastructure;
using Aufgabe3_RestfulApi.Mapper;
using Aufgabe3_RestfulApi.Model;
using Microsoft.EntityFrameworkCore;

namespace Aufgabe3_RestfulApi.Services;

public class MakerspaceService(MakerspaceContext db) : IMakerspaceService
{
    public async Task<PagedResultDto<DeviceListDto>> GetDevicePaged(DeviceType? deviceType, DeviceStatus? deviceStatus, string? name, int page, int pageSize, CancellationToken ct)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        IQueryable<Device> devices = db.Devices.AsNoTracking();

        if (deviceType != null)
        {
            devices = devices.Where(d => d.DeviceType == deviceType.Value);
        }
        
        if (deviceStatus != null)
        {
            devices = devices.Where(d => d.Status == deviceStatus.Value);
        }

        if (!string.IsNullOrWhiteSpace(name))
        {
            devices = devices.Where(d => d.Name.Contains(name));
        }
        
        int totalCount = await devices.CountAsync(ct);

        List<DeviceListDto> items = await devices
            .OrderBy(d => d.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(MakerspaceMapper.DeviceListProjection)
            .ToListAsync(ct);

        return MakerspaceMapper.ToPagedDto(items, page, pageSize, totalCount);
    }

    public async Task<DeviceDetailDto> GetDeviceDetail(int id, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task<DeviceDetailDto> CreateDevice(CreateDeviceCmd cmd, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task<DeviceDetailDto> UpdateDeviceStatus(int id, UpdateDeviceStatusCmd cmd, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task<DeviceDetailDto> DeleteDevice(int id, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task<ReservationDto> CreateReservation(CreateReservationCmd cmd, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task<ReservationDto> PutReservation(int id, CompleteReservationCmd cmd, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task<MemberReservationDto> GetMemberReservation(int id, ReservationStatus? status, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<MaintenanceDto>> GetMaintenance(DeviceType? type, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task<MaintenanceDto> CreateMaintenanceTicket(CreateMaintenanceTicketCmd cmd, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<DeviceTypeStatsDto>> GetDeviceTypInfo(CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
