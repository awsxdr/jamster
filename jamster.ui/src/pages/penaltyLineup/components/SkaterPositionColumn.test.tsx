import { i18nMock } from "@/test/mocks";
import { render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";

import { v4 as uuid4 } from 'uuid';

import { GameSkater, LineupPosition, TeamSide } from "@/types";
import { SkaterPositionColumn } from "./SkaterPositionColumn";
import userEvent from "@testing-library/user-event";

vi.mock('@/hooks', () => ({
    usePenaltyBoxState: vi.fn(),
    useI18n: vi.fn(() => i18nMock),
}));

const testRoster: GameSkater[] = Array.from(new Array(10))
    .map((_, i) => ({ id: uuid4(), number: `${i + 1}`, name: `Test Skater ${i + 1}`, isSkating: true }))
    .sort((a, b) => a.number.localeCompare(b.number));

describe('SkaterPositionColumn', () => {
    it('renders button with position per skater', () => {
        render(
            <SkaterPositionColumn 
                teamSide={TeamSide.Home} 
                position={LineupPosition.Blocker} 
                skaters={testRoster} 
                selectedSkaters={[]} 
                offTrackSkaters={[]}
                injuredSkaters={[]}
                currentJam
            />
        );

        expect(screen.getAllByRole('button', { name: 'Blocker' })).toHaveLength(testRoster.length);
    });

    it.each([LineupPosition.Blocker, LineupPosition.Jammer, LineupPosition.Pivot])('renders button in danger state if skater is injured and position is not bench and is current jam', (position) => {
        render(
            <SkaterPositionColumn
                teamSide={TeamSide.Home} 
                position={position} 
                skaters={[testRoster[0]]} 
                selectedSkaters={[testRoster[0].id]} 
                offTrackSkaters={[]}
                injuredSkaters={[testRoster[0].id]}
                currentJam
            />
        );

        expect(screen.getByRole('button', { name: position.toString() })).toHaveAttribute('aria-invalid', 'true');
    });

    it('does not render button in danger state if skater is injured and position is bench and is current jam', () => {
        render(
            <SkaterPositionColumn
                teamSide={TeamSide.Home} 
                position={LineupPosition.Bench} 
                skaters={[testRoster[0]]} 
                selectedSkaters={[testRoster[0].id]} 
                offTrackSkaters={[]}
                injuredSkaters={[testRoster[0].id]}
                currentJam
            />
        );

        expect(screen.getByRole('button', { name: 'Bench' })).toHaveAttribute('aria-invalid', 'false');
    });

    it.each([LineupPosition.Blocker, LineupPosition.Jammer, LineupPosition.Pivot])('does not render button in danger state if skater is injured and position is not bench and is not current jam', (position) => {
        render(
            <SkaterPositionColumn
                teamSide={TeamSide.Home} 
                position={position} 
                skaters={[testRoster[0]]} 
                selectedSkaters={[testRoster[0].id]} 
                offTrackSkaters={[]}
                injuredSkaters={[testRoster[0].id]}
            />
        );

        expect(screen.getByRole('button', { name: position.toString() })).toHaveAttribute('aria-invalid', 'false');
    });

    it.each([LineupPosition.Blocker, LineupPosition.Jammer, LineupPosition.Pivot])('renders button in danger state if skater is off track and position is not bench and is current jam', (position) => {
        render(
            <SkaterPositionColumn
                teamSide={TeamSide.Home} 
                position={position} 
                skaters={[testRoster[0]]} 
                selectedSkaters={[testRoster[0].id]} 
                offTrackSkaters={[testRoster[0].id]}
                injuredSkaters={[]}
                currentJam
            />
        );

        expect(screen.getByRole('button', { name: position.toString() })).toHaveAttribute('aria-invalid', 'true');
    });

    it('does not render button in danger state if skater is off track and position is bench and is current jam', () => {
        render(
            <SkaterPositionColumn
                teamSide={TeamSide.Home} 
                position={LineupPosition.Bench} 
                skaters={[testRoster[0]]} 
                selectedSkaters={[testRoster[0].id]} 
                offTrackSkaters={[testRoster[0].id]}
                injuredSkaters={[]}
                currentJam
            />
        );

        expect(screen.getByRole('button', { name: 'Bench' })).toHaveAttribute('aria-invalid', 'false');
    });

    it.each([LineupPosition.Blocker, LineupPosition.Jammer, LineupPosition.Pivot])('does not render button in danger state if skater is off track and position is not bench and is not current jam', (position) => {
        render(
            <SkaterPositionColumn
                teamSide={TeamSide.Home} 
                position={position} 
                skaters={[testRoster[0]]} 
                selectedSkaters={[testRoster[0].id]} 
                offTrackSkaters={[testRoster[0].id]}
                injuredSkaters={[]}
            />
        );

        expect(screen.getByRole('button', { name: position.toString() })).toHaveAttribute('aria-invalid', 'false');
    });

    it('when clicked invokes onSkaterClicked', async () => {

        const onSkaterClicked = vi.fn();

        render(
            <SkaterPositionColumn
                teamSide={TeamSide.Home} 
                position={LineupPosition.Blocker} 
                skaters={[testRoster[0]]} 
                selectedSkaters={[]} 
                offTrackSkaters={[]}
                injuredSkaters={[]}
                currentJam
                onSkaterClicked={onSkaterClicked}
            />
        );

        await userEvent.click(screen.getByRole('button', { name: 'Blocker' }));

        expect(onSkaterClicked).toHaveBeenCalledOnce();
    })
});