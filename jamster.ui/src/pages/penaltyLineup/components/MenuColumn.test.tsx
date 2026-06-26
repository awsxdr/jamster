import { beforeEach, describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";

import { v4 as uuid4 } from 'uuid';

import { GameSkater, LineupPosition, StringMap, TeamSide } from "@/types";
import { MenuColumn, MenuColumnProps } from "./MenuColumn";
import { RowMenuProps } from "./RowMenu";
import { usePenaltyBoxState } from "@/hooks";
import { i18nMock } from "@/test/mocks";

vi.mock('@/hooks', () => ({
    useI18n: vi.fn(() => i18nMock),
    usePenaltyBoxState: vi.fn(),
}));

const testRoster: GameSkater[] = Array.from(new Array(10))
    .map((_, i) => ({ id: uuid4(), number: `${i + 1}`, name: `Test Skater ${i + 1}`, isSkating: true }))
    .sort((a, b) => a.number.localeCompare(b.number));

const testPositions: StringMap<LineupPosition> = testRoster.reduce((t, s) => ({ ...t, [s.id]: LineupPosition.Bench }), {});

const DefaultMenuColumn = (props: Partial<MenuColumnProps>) => (
    <MenuColumn
        teamSide={TeamSide.Home}
        skaters={testRoster} 
        skaterPositions={testPositions} 
        injuredSkaters={[]} 
        {...props}
    />
);

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

beforeEach(() => {
    vi.clearAllMocks();
});

describe('MenuColumn', () => {
    it('enables notes button when skater is not on bench', () => {
        const positions = {
            ...testPositions,
            [testRoster[1].id]: LineupPosition.Blocker,
            [testRoster[2].id]: LineupPosition.Jammer,
            [testRoster[3].id]: LineupPosition.Pivot,
        };

        render(<DefaultMenuColumn skaterPositions={positions} />);
        
        testRoster.forEach((s, i) => {
            expect(vi.mocked(MockRowMenu)).toHaveBeenNthCalledWith(
                i + 1,
                expect.objectContaining({ disableNotes: positions[s.id] === LineupPosition.Bench }),
                expect.anything()
            );
        });
    });

    it('renders substitute button when skater is in box', () => {
        vi.mocked(usePenaltyBoxState).mockReturnValue({ skaters: [testRoster[0].id], queuedSkaters: [] });

        render(<DefaultMenuColumn skaters={[testRoster[0]]} />);

        expect(vi.mocked(MockRowMenu)).toHaveBeenCalledWith(
            expect.objectContaining({ inBox: true }),
            expect.anything()
        );
    });

    it('calls onInjuryAdded when injury added', async () => {
        const onInjuryAdded = vi.fn();
        
        render(<DefaultMenuColumn skaters={[testRoster[0]]} onInjuryAdded={onInjuryAdded} />);

        await userEvent.click(screen.getByRole('button', { name: 'Add injury' }));

        expect(onInjuryAdded).toHaveBeenCalledOnce();
    });

    it('calls onInjuryRemoved when injury removed', async () => {
        const onInjuryRemoved = vi.fn();

        render(<DefaultMenuColumn skaters={[testRoster[0]]} onInjuryRemoved={onInjuryRemoved} />);

        await userEvent.click(screen.getByRole('button', { name: 'Remove injury' }));

        expect(onInjuryRemoved).toHaveBeenCalledOnce();
    });
});