import { render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { RowMenu } from "./RowMenu";
import { i18nMock } from "@/test/mocks";
import userEvent from "@testing-library/user-event";

vi.mock('@/hooks', () => ({
    useI18n: vi.fn(() => i18nMock),
}));


describe('RowMenu', () => {
    it('renders add injury when no injury active', async () => {
        render(
            <RowMenu injuryActive={false}>
                <button>Open</button>
            </RowMenu>
        );

        await userEvent.click(screen.getByRole('button', { name: 'Open' }));

        expect(screen.getByRole('menuitem', { name: 'AddInjury' })).toBeVisible();
    });

    it('does not render add injury when injury active', async () => {
        render(
            <RowMenu injuryActive={true}>
                <button>Open</button>
            </RowMenu>
        );

        await userEvent.click(screen.getByRole('button', { name: 'Open' }));

        expect(screen.queryByRole('menuitem', { name: 'AddInjury' })).not.toBeInTheDocument();
    });

    it('renders remove injury when injury active', async () => {
        render(
            <RowMenu injuryActive={true}>
                <button>Open</button>
            </RowMenu>
        );

        await userEvent.click(screen.getByRole('button', { name: 'Open' }));

        expect(screen.getByRole('menuitem', { name: 'RemoveInjury' })).toBeVisible();
    });

    it('does not render remove injury when injury active', async () => {
        render(
            <RowMenu injuryActive={false}>
                <button>Open</button>
            </RowMenu>
        );

        await userEvent.click(screen.getByRole('button', { name: 'Open' }));

        expect(screen.queryByRole('menuitem', { name: 'RemoveInjury' })).not.toBeInTheDocument();
    });

    it.each([true, false])('renders add notes disabled according to disableNotes property', async disableNotes => {
        render(
            <RowMenu disableNotes={disableNotes}>
                <button>Open</button>
            </RowMenu>
        );

        await userEvent.click(screen.getByRole('button', { name: 'Open' }));

        if(disableNotes) {
            expect(screen.getByRole('menuitem', { name: 'Notes' })).toHaveAttribute('aria-disabled', 'true')
        } else {
            expect(screen.getByRole('menuitem', { name: 'Notes' })).not.toHaveAttribute('aria-disabled');
        }
    });

    it('invokes onInjuryAdded when add injury clicked', async () => {
        const onInjuryAdded = vi.fn();

        render(
            <RowMenu onInjuryAdded={onInjuryAdded}>
                <button>Open</button>
            </RowMenu>
        );

        await userEvent.click(screen.getByRole('button', { name: 'Open' }));

        await userEvent.click(screen.getByRole('menuitem', { name: 'AddInjury' }));

        expect(onInjuryAdded).toHaveBeenCalledOnce();
    });

    it('invokes onInjuryRemoved when remove injury clicked', async () => {
        const onInjuryRemoved = vi.fn();

        render(
            <RowMenu onInjuryRemoved={onInjuryRemoved} injuryActive={true}>
                <button>Open</button>
            </RowMenu>
        );

        await userEvent.click(screen.getByRole('button', { name: 'Open' }));

        await userEvent.click(screen.getByRole('menuitem', { name: 'RemoveInjury' }));

        expect(onInjuryRemoved).toHaveBeenCalledOnce();
    });

    it('renders substitute button when skater in box', async () => {
        render(
            <RowMenu inBox>
                <button>Open</button>
            </RowMenu>
        );

        await userEvent.click(screen.getByRole('button', { name: 'Open' }));

        expect(screen.getByRole('menuitem', { name: 'Substitute' })).toBeVisible();
    });
});