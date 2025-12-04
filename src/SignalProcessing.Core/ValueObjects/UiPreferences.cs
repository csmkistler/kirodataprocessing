namespace SignalProcessing.Core.ValueObjects;

public record UiPreferences(
    double ChartZoom,
    ChartPan ChartPan
);

public record ChartPan(double X, double Y);
