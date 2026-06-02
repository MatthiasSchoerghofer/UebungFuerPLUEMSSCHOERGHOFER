using System;
using System.Collections.Generic;
using Aufgabe3_RestfulApi.Model;

namespace Aufgabe3_RestfulApi.Cmds;

public record CreateDeviceCmd(
    string InventoryNumber,
    string Name,
    DeviceType DeviceType,
    decimal ReplacementValue,
    DateTime PurchasedOn,
    Room Room,
    IReadOnlyList<string> TagIds,
    string? Note);

public record UpdateDeviceStatusCmd(
    DeviceStatus Status,
    string? Note);

public record CreateReservationCmd(
    int DeviceId,
    int MemberId,
    DateTime ReservedFrom,
    DateTime ReservedUntil,
    string Purpose);

public record CompleteReservationCmd(
    DateTime ReturnedAt,
    string? ReturnNote);

public record CreateMaintenanceTicketCmd(
    int DeviceId,
    TicketSeverity Severity,
    string Description);

public record UpdateMemberLockCmd(bool IsLocked);
