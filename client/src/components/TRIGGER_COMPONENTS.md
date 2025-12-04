# Trigger Components

This document describes the trigger monitoring components implemented for the signal processing visualization application.

## Components

### TriggerInput

A component that allows users to:
- Configure a threshold value for monitoring
- Enable/disable threshold monitoring
- Enter numeric values to check against the threshold
- Receive immediate feedback when values exceed the threshold

**Props:**
- `onCheckValue`: Async function to check a value against the threshold
- `onConfigureThreshold`: Async function to configure the threshold
- `isLoading`: Optional boolean to indicate loading state

**Features:**
- Real-time validation of numeric inputs
- Visual feedback for threshold exceedance
- Separate configuration and checking sections
- Error handling and user-friendly messages

**Validates Requirements:** 5.1, 5.2, 5.4

### EventList

A component that displays trigger events in a table format with:
- Reverse chronological ordering (most recent first)
- Event value, threshold, and timestamp for each event
- Auto-refresh capability
- Manual refresh button
- Empty state message

**Props:**
- `onFetchEvents`: Async function to fetch events from the API
- `autoRefreshInterval`: Optional interval in milliseconds for auto-refresh (default: 5000ms)
- `maxEvents`: Optional maximum number of events to display (default: 50)

**Features:**
- Automatic sorting by timestamp (newest first)
- Formatted timestamps with relative time display
- Pagination/limiting of displayed events
- Auto-refresh with configurable interval
- Responsive table layout

**Validates Requirements:** 5.3, 5.5

## Property-Based Tests

### Property 11: Event Display
**Validates:** Requirements 5.3

For any emitted event, the Event List should display:
1. The event value
2. The threshold value
3. The timestamp

The test generates random trigger events and verifies that all three pieces of information are displayed in the UI.

### Property 12: Event Ordering
**Validates:** Requirements 5.5

For any sequence of triggered events, the Event List should display them in reverse chronological order with the most recent event at the top.

The test generates arrays of events with random timestamps and verifies they are sorted correctly in the UI.

## Usage Example

```tsx
import { TriggerInput, EventList } from './components';
import api from './services/api';

function App() {
  const handleCheckValue = async (request) => {
    return await api.triggers.checkValue(request);
  };

  const handleConfigureThreshold = async (request) => {
    await api.triggers.configure(request);
  };

  const handleFetchEvents = async () => {
    return await api.triggers.getEvents();
  };

  return (
    <div>
      <TriggerInput
        onCheckValue={handleCheckValue}
        onConfigureThreshold={handleConfigureThreshold}
      />
      <EventList
        onFetchEvents={handleFetchEvents}
        autoRefreshInterval={5000}
        maxEvents={50}
      />
    </div>
  );
}
```

## Testing

To run the property-based tests:

```bash
npm test -- EventList.test.tsx --run
```

The tests use `fast-check` for property-based testing with 100 iterations per property as specified in the design document.

## Implementation Notes

1. **Validation**: Both components implement real-time validation with immediate user feedback
2. **Error Handling**: Graceful error handling with user-friendly messages
3. **Performance**: EventList implements virtual scrolling considerations for large datasets
4. **Accessibility**: Semantic HTML with proper table structure and labels
5. **Styling**: Inline styles matching the existing component patterns for consistency

## Requirements Coverage

- ✅ 5.1: Threshold comparison functionality
- ✅ 5.2: Event emission on threshold exceeded
- ✅ 5.3: Event display with value and timestamp
- ✅ 5.4: User-configurable threshold values
- ✅ 5.5: Chronological event ordering (most recent first)
