# DnetTimeline

A vertical history or process made of an ordered list and declarative events.
Events accept rich content, dates, decorative bullets and secondary metadata.

```razor
@using Dnet.Blazor.Components.Timeline

<DnetTimeline AriaLabel="Order progress">
    <DnetTimelineItem Title="Confirmed"
                      Status="DnetTimelineItemStatus.Completed"
                      Timestamp="Today, 09:15"
                      DateTime="2026-10-16T09:15:00+02:00">
        <ChildContent><p>Your payment has been received.</p></ChildContent>
    </DnetTimelineItem>
    <DnetTimelineItem Title="Preparing" Status="DnetTimelineItemStatus.Current">
        <ChildContent><p>We are getting your order ready.</p></ChildContent>
    </DnetTimelineItem>
    <DnetTimelineItem Title="Delivery" Status="DnetTimelineItemStatus.Pending" />
</DnetTimeline>
```

Put `DnetTimelineItem` components directly inside `DnetTimeline`. Render them with
`foreach`, `@if` and `@key` as usual; items need no registration or JavaScript.
Keep their DOM order chronological or in process order, including alternating
layouts. The component never sorts events or infers their status from position.

## Layouts

`Alignment` accepts `DnetTimelineAlignment.Start` (default), `End` and `Alternate`.
Start and End follow the text direction, so `dir="rtl"` mirrors the layout.
Alternate centers the rail and places successive events on opposite sides. At
viewport widths of 640 pixels or less it becomes a single column at logical start.

```razor
<DnetTimeline Alignment="DnetTimelineAlignment.Alternate" AriaLabel="Project milestones">
    <DnetTimelineItem Title="First release" Status="DnetTimelineItemStatus.Completed">
        <ChildContent><p>Our first customers joined.</p></ChildContent>
        <OppositeContent>January 2026</OppositeContent>
    </DnetTimelineItem>
    <DnetTimelineItem Title="The next chapter" Status="DnetTimelineItemStatus.Current">
        <BulletContent>A</BulletContent>
        <ChildContent><p>Ana is collecting feedback.</p></ChildContent>
        <OppositeContent>Product team</OppositeContent>
    </DnetTimelineItem>
</DnetTimeline>
```

`OppositeContent` appears across the rail in Alternate. It follows the event
content in Start, End and the narrow-screen layout, so metadata remains readable
when there is little horizontal space. `Timestamp` always follows the event body.

## Parameters

| DnetTimeline | Purpose |
| --- | --- |
| `Alignment` | Start, End or Alternate. |
| `AriaLabel` | Accessible list name; defaults to `Timeline`. Set null to omit it. |
| `ChildContent` | The timeline items. |
| `AdditionalAttributes` | Attributes on the `ol`, including `id`, `class`, `style` and `dir`. |

| DnetTimelineItem | Purpose |
| --- | --- |
| `Title` | Optional plain-text event title. |
| `Status` | Completed, Current or Pending; defaults to Pending. |
| `StatusLabel` | Localized status for assistive technology; defaults to the enum name. |
| `Timestamp` | Optional human-readable date or time. |
| `DateTime` | Optional ISO date or time. When supplied, the timestamp renders as `time` with this `datetime` attribute; otherwise it renders as `span`. |
| `BulletContent` | Decorative custom bullet; replaces the built-in status symbol. |
| `OppositeContent` | Optional secondary metadata. |
| `ChildContent` | Optional rich content, including interactive elements. |
| `AdditionalAttributes` | Attributes on the `li`, including `id`, `class` and `style`. |

Each Completed event draws an accented connector toward the next event; Current
and Pending events draw a neutral connector. The final event has no connector.
Changing a status updates the appearance without changing content or focus.

## Accessibility

The root is an `ol` with `role="list"` so its list semantics remain available when
visual numbering is removed. Each event is an `li`. Current events expose
`aria-current="step"`; choose at most one Current event for a single process.
The status is also present as visually hidden text, and `StatusLabel` supports
localization. For example, use `StatusLabel="En curso"` on the current event.

Bullets are decorative and have `aria-hidden="true"`. Put meaningful information
in the title, content or metadata rather than only in a bullet, and do not place
interactive controls inside `BulletContent`. Links and controls in event content
keep their native keyboard behavior and receive a visible focus indicator.
No special keyboard interaction or artificial tab stops are introduced.

`Title` is styled text rather than a heading, allowing the timeline to fit any
heading hierarchy. Put an appropriate heading in `ChildContent` if your document
needs heading navigation. Provide valid ISO values for `DateTime`; the library
does not parse dates or choose a locale for the supplied display label. Labels
such as “Now” or “Next update” without `DateTime` render as ordinary text rather
than an invalid machine-readable time.

## Styling

Set tokens on `:root`, a surrounding container, or the `DnetTimeline` instance.
They inherit through the timeline and follow the active light or dark theme.

```razor
<DnetTimeline style="--dnet-timeline-bullet-size: 30px; --dnet-timeline-gap: 36px;">
    ...
</DnetTimeline>
```

| Token | Default |
| --- | --- |
| `--dnet-timeline-bullet-size` | Six semantic space units (24px). |
| `--dnet-timeline-line-width` | 2px. |
| `--dnet-timeline-gap` | Seven semantic space units (28px). |
| `--dnet-timeline-content-gap` | `--dnet-sys-space-4`. |
| `--dnet-timeline-line-color` | `--dnet-sys-border-strong`. |
| `--dnet-timeline-accent-color` | `--dnet-sys-primary-emphasis`. |
| `--dnet-timeline-accent-foreground` | `--dnet-sys-on-primary`. |
| `--dnet-timeline-bullet-background` | `--dnet-sys-surface`. |
| `--dnet-timeline-foreground` | `--dnet-sys-on-surface`. |
| `--dnet-timeline-muted-foreground` | `--dnet-sys-on-surface-muted`. |
| `--dnet-timeline-title-font-size` | `--dnet-sys-text-lg`. |
| `--dnet-timeline-font-size` | `--dnet-sys-text-md`. |

The body, title and metadata wrap long text without expanding the page width.
There are no animations or dependencies on icon fonts.

The sample page is available at `/Timeline`.
