using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Aufgabe3_RestfulApi.Cmds;
using Aufgabe3_RestfulApi.Dtos;
using Aufgabe3_RestfulApi.Model;

namespace Aufgabe3_RestfulApi.Mapper;

public static class MakerspaceMapper
{
    public static readonly Expression<Func<Device, DeviceListDto>> DeviceListProjection =
        d => new DeviceListDto(
            d.Id,
            d.InventoryNumber,
            d.Name,
            d.DeviceType,
            d.Status,
            d.Room.RoomNumber,
            d.ReplacementValue);

    public static PagedResultDto<TDto> ToPagedDto<TDto>(
        IReadOnlyList<TDto> items,
        int page,
        int pageSize,
        int totalCount)
    {
        return new PagedResultDto<TDto>(
            items,
            page,
            pageSize,
            totalCount);
    }
    
    public static DeviceDetailDto ToDetailDto(Device device)
    {
        return new DeviceDetailDto(
            device.Id,
            device.InventoryNumber,
            device.Name,
            device.DeviceType,
            device.Status,
            device.ReplacementValue,
            device.PurchasedOn,
            device.Room.RoomNumber,
            device.Tags.Select(t => t.Text).Distinct().ToList(),
            device.MaintenanceTickets.Count
        );
    }

    public static Device ToDeviceEntity(CreateDeviceCmd cmd)
    {
        return new Device(
            cmd.InventoryNumber,
            cmd.Name,
            cmd.DeviceType,
            cmd.ReplacementValue,
            cmd.PurchasedOn,
            cmd.Room
            );
    }
}
