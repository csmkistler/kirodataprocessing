using SignalProcessing.Core.Entities;
using SignalProcessing.Core.ValueObjects;

namespace SignalProcessing.Core.Interfaces;

public interface ITriggerComponent
{
    void Configure(TriggerConfig config);
    Task<TriggerEvent?> CheckValue(double value);
    Task<List<TriggerEvent>> GetEvents();
}
