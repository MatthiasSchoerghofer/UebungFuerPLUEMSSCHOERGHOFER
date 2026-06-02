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

    public async Task<DeviceDetailDto?> GetDeviceDetail(int id, CancellationToken ct)
    {
        Device? result = await db.Devices
            .AsNoTracking()
            .Include(d => d.Room)
            .Include(d => d.Tags)
            .Include(d => d.MaintenanceTickets)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken: ct);

        return result != null ? MakerspaceMapper.ToDetailDto(result) : null;
    }

    public async Task<DeviceDetailDto> CreateDevice(CreateDeviceCmd cmd, CancellationToken ct)
    {
        if (await db.Devices.AnyAsync(d => d.InventoryNumber == cmd.InventoryNumber, ct))
        {
            throw new InvalidOperationException("Inventory number already exists.");
        }

        if (cmd.ReplacementValue < 0)
        {
            throw new ArgumentException("Replacement value must not be negative.");
        }

        Room room = await db.Rooms.FirstOrDefaultAsync(r => r.RoomNumber == cmd.Room.RoomNumber, ct)
            ?? throw new KeyNotFoundException("Room was not found.");

        List<EquipmentTag> tags = await db.EquipmentTags
            .Where(t => cmd.TagIds.Contains(t.Id))
            .ToListAsync(ct);

        if (tags.Count != cmd.TagIds.Distinct().Count())
        {
            throw new KeyNotFoundException("One or more tags were not found.");
        }

        var newDevice = new Device(
            cmd.InventoryNumber,
            cmd.Name,
            cmd.DeviceType,
            cmd.ReplacementValue,
            cmd.PurchasedOn,
            room)
        {
            Note = cmd.Note
        };

        newDevice.Tags.AddRange(tags);

        db.Devices.Add(newDevice);
        await db.SaveChangesAsync(ct);

        return await GetDeviceDetail(newDevice.Id, ct)
            ?? throw new InvalidOperationException("Created device could not be loaded.");
    }

    public async Task<DeviceDetailDto> UpdateDeviceStatus(int id, UpdateDeviceStatusCmd cmd, CancellationToken ct)
    {
        Device? result = await db.Devices.FirstOrDefaultAsync(d => d.Id == id, ct);
        if (result == null) { throw new KeyNotFoundException("Device was not found."); }
        
        result.Status = cmd.Status;
        result.Note = cmd.Note;
        
        await db.SaveChangesAsync(ct);
        
        return await GetDeviceDetail(id, ct)
            ?? throw new KeyNotFoundException("Device was not found.");
    }

    public async Task<DeviceDetailDto> DeleteDevice(int id, CancellationToken ct)
    {
        Device? result = await db.Devices
            .Include(d => d.Room)
            .Include(d => d.Tags)
            .Include(d => d.MaintenanceTickets)
            .Include(d => d.Reservations)
            .FirstOrDefaultAsync(d => d.Id == id, ct);

        if (result == null) { throw new KeyNotFoundException("Device was not found."); }

        if (result.Reservations.Any(r => r.Status is ReservationStatus.Active or ReservationStatus.Planned))
        {
            throw new InvalidOperationException("Device cannot be deleted while it has active or planned reservations.");
        }
        
        db.Devices.Remove(result);
        
        await db.SaveChangesAsync(ct);
        
        return MakerspaceMapper.ToDetailDto(result);
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
