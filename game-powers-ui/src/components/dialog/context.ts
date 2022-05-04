import { createContext, useContext, useEffect, useState } from 'react';
import { DialogService } from './dialog.service';
import { DialogOptions, DialogRenderInfo } from './DialogOptions';

const context = createContext<DialogService | null>(new DialogService());

export function useDisplayedDialogs() {
	const dialogService = useContext(context);
	if (!dialogService) throw new Error('May only display dialogs within a DialogProvider');

	const [state, setState] = useState<DialogRenderInfo[]>([]);

	useEffect(() => {
		const output$ = dialogService.dialog$;
		const subscription = output$.subscribe((value) => {
			setState(value);
		});
		return () => {
			subscription.unsubscribe();
		};
		// eslint-disable-next-line react-hooks/exhaustive-deps
	}, []); // immutable forever

	return state;
}

export function useDialog() {
	const dialogService = useContext(context);
	if (!dialogService) throw new Error('May only use a dialog within a DialogProvider');
	return async <T>(dialogOptions: DialogOptions<T>) => {
		return dialogService.awaitDialog<T>(dialogOptions);
	};
}

export const DialogProvider = context.Provider;
