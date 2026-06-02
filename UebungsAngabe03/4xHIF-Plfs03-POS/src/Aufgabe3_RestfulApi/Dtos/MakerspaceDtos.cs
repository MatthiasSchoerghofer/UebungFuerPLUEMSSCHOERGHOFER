using System;
using System.Collections.Generic;
using Aufgabe3_RestfulApi.Model;

namespace Aufgabe3_RestfulApi.Dtos;

public record PagedResultDto<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount);

public record DeviceListDto(
    int Id,
    string InventoryNumber,
    string Name,
    DeviceType DeviceType,
    DeviceStatus Status,
    string RoomNumber,
    decimal ReplacementValue);

public record DeviceDetailDto(
    int Id,
    string InventoryNumber,
    string Name,
    DeviceType DeviceType,
    DeviceStatus Status,
    decimal ReplacementValue,
    DateTime PurchasedOn,
    string RoomNumber,
    IReadOnlyList<string> Tags,
    int OpenTicketCount);

public record ReservationDto(
    int Id,
    int DeviceId,
    string DeviceName,
    int MemberId,
    string MemberName,
    DateTime ReservedFrom,
    DateTime ReservedUntil,
    DateTime? ReturnedAt,
    ReservationStatus Status,
    string Purpose);

public record MemberReservationDto(
    int MemberId,
    string MemberName,
    bool IsLocked,
    IReadOnlyList<ReservationDto> Reservations);

public record DeviceTypeStatsDto(
    DeviceType DeviceType,
    int TotalDevices,
    int AvailableDevices,
    decimal AverageReplacementValue);

public record MaintenanceDto(
    int TicketId,
    DateTime CreatedAt,
    DateTime ClosedAt,
    TicketSeverity Severity,
    bool IsOpen,
    DeviceType DeviceType
);