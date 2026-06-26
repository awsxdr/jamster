import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { beforeEach, describe, expect, it, vi } from 'vitest';

import { v4 as uuid4 } from 'uuid';

import { usePenaltyBoxState, useRulesState } from '@/hooks';
import { GameSkater, LineupPosition, PeriodEndBehavior, RulesState, StringMap, TeamSide, TimeoutPeriodClockStopBehavior, TimeoutResetBehavior } from '@/types';
import { SkaterNumberColumn, SkaterNumberColumnProps } from './SkaterNumberColumn';
import { RowMenuProps } from './RowMenu';
import { i18nMock } from '@/test/mocks';

vi.mock('@/hooks', () => ({
    useRulesState: vi.fn(),
    useI18n: vi.fn(() => i18nMock),
    usePenaltyBoxState: vi.fn(),
}));

const MockRowMenu = vi.hoisted(() => 
    vi.fn(({ children, onInjuryAdded, onInjuryRemoved }: React.PropsWithChildren<RowMenuProps>) => (
        <>
            <button onClick={onInjuryAdded}>Add injury</button>
            <button onClick={onInjuryRemoved}>Remove injury</button>
            {children}
        </>
    )));

vi.mock('@pages/penaltyLineup/components', () => ({
    RowMenu: MockRowMenu,
}));

const testRoster: GameSkater[] = Array.from(new Array(10))
    .map((_, i) => ({ id: uuid4(), number: `${i + 1}`, name: `Test Skater ${i + 1}`, isSkating: true }))
    .sort((a, b) => a.number.localeCompare(b.number));

const testPositions: StringMap<LineupPosition> = testRoster.reduce((t, s) => ({ ...t, [s.id]: LineupPosition.Bench }), {});

const testRules: RulesState = {
    rules: {
        periodRules: {
            periodCount: 2,
            durationInSeconds: 30 * 60,
            periodEndBehavior: PeriodEndBehavior.AnytimeOutsideJam,
        },
        jamRules: {
            durationInSeconds: 2 * 60,
            resetJamNumbersBetweenPeriods: true,
        },
        lineupRules: {
            durationInSeconds: 30,
            overtimeDurationInSeconds: 60,
        },
        timeoutRules: {
            teamTimeoutAllowance: 3,
            teamTimeoutDurationInSeconds: 60,
            periodClockBehavior: TimeoutPeriodClockStopBehavior.All,
            resetBehavior: TimeoutResetBehavior.Never,
        },
        intermissionRules: {
            durationInSeconds: 15 * 60,
        },
        penaltyRules: {
            foulOutPenaltyCount: 7,
        },
    }
};

const DefaultSkaterNumberColumn = (props: Partial<SkaterNumberColumnProps>) => (
    <SkaterNumberColumn
        teamSide={TeamSide.Home}
        skaters={testRoster} 
        skaterPositions={testPositions} 
        offTrackSkaters={[]} 
        injuredSkaters={[]} 
        {...props}
    />
);

beforeEach(() => {
    vi.clearAllMocks();
});

describe('SkaterNumberColumn', () => {
    it('renders nothing when rules are not loaded', () => {
        vi.mocked(useRulesState).mockReturnValue(undefined);

        const { container } = render(<DefaultSkaterNumberColumn />);

        expect(container).toBeEmptyDOMElement();
    });

    it('renders skater numbers when rules are loaded', () => {
        vi.mocked(useRulesState).mockReturnValue(testRules);

        render(<DefaultSkaterNumberColumn />);

        testRoster.forEach(s => {
            expect(screen.getAllByText(s.number)).toHaveLength(2);
        });
    });

    it('renders bandage icon when skater is injured', () => {
        vi.mocked(useRulesState).mockReturnValue(testRules);

        render(<DefaultSkaterNumberColumn injuredSkaters={[testRoster[1].id]} />);

        expect(screen.getAllByRole('img', { name: 'Injured' }));
    });

    it('renders warning icon when skater is off track', () => {
        vi.mocked(useRulesState).mockReturnValue(testRules);

        render(<DefaultSkaterNumberColumn offTrackSkaters={[testRoster[1].id]} />);

        expect(screen.getAllByRole('img', { name: 'OffTrack' }));
    });

    it('enables notes button when skater is not on bench', () => {
        vi.mocked(useRulesState).mockReturnValue(testRules);

        const positions = {
            ...testPositions,
            [testRoster[1].id]: LineupPosition.Blocker,
            [testRoster[2].id]: LineupPosition.Jammer,
            [testRoster[3].id]: LineupPosition.Pivot,
        };

        render(<DefaultSkaterNumberColumn skaterPositions={positions} />);
        
        testRoster.forEach((s, i) => {
            expect(vi.mocked(MockRowMenu)).toHaveBeenNthCalledWith(
                i + 1,
                expect.objectContaining({ disableNotes: positions[s.id] === LineupPosition.Bench }),
                expect.anything()
            );
        });
    });

    it('renders substitute button when skater is in box', () => {
        vi.mocked(useRulesState).mockReturnValue(testRules);
        vi.mocked(usePenaltyBoxState).mockReturnValue({ skaters: [testRoster[0].id], queuedSkaters: [] });

        render(<DefaultSkaterNumberColumn skaters={[testRoster[0]]} />);

        expect(vi.mocked(MockRowMenu)).toHaveBeenCalledWith(
            expect.objectContaining({ inBox: true }),
            expect.anything()
        );
    });

    it('calls onInjuryAdded when injury added', async () => {
        vi.mocked(useRulesState).mockReturnValue(testRules);

        const onInjuryAdded = vi.fn();
        
        render(<DefaultSkaterNumberColumn skaters={[testRoster[0]]} onInjuryAdded={onInjuryAdded} />);

        await userEvent.click(screen.getByRole('button', { name: 'Add injury' }));

        expect(onInjuryAdded).toHaveBeenCalledOnce();
    });

    it('calls onInjuryRemoved when injury removed', async () => {
        vi.mocked(useRulesState).mockReturnValue(testRules);

        const onInjuryRemoved = vi.fn();

        render(<DefaultSkaterNumberColumn skaters={[testRoster[0]]} onInjuryRemoved={onInjuryRemoved} />);

        await userEvent.click(screen.getByRole('button', { name: 'Remove injury' }));

        expect(onInjuryRemoved).toHaveBeenCalledOnce();
    });
});