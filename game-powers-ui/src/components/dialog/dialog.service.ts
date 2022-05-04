import { v4 as uuid } from 'uuid';
import { BehaviorSubject, Observable, Subject } from 'rxjs';
import { map } from 'rxjs/operators';
import { DialogOptions, DialogRenderInfo } from './DialogOptions';

type Dialog = {
	options: DialogOptions;
	renderInfo: DialogRenderInfo;
	resolved: Observable<unknown>;
};

export class DialogService {
	private readonly dialogSubject = new BehaviorSubject<Dialog[]>([]);

	public readonly dialog$ = this.dialogSubject.pipe(map((dialogs) => dialogs.map((dialog) => dialog.renderInfo)));

	awaitDialog<T>(dialogOptions: DialogOptions<T>): Promise<T> {
		const subject = new Subject<T>();

		const resolver = (result: T) => {
			subject.next(result);
			subject.complete();
		};

		const { cancellationValue, renderer } = dialogOptions;

		const dialog: Dialog = {
			options: dialogOptions,
			resolved: subject.asObservable(),
			renderInfo: {
				key: uuid(),
				renderer: () => renderer(resolver),
				onCancel: cancellationValue !== undefined ? () => resolver(cancellationValue) : undefined,
				size: dialogOptions.size,
				title: dialogOptions.title,
			},
		};

		const removeDialog = () => {
			this.dialogSubject.next(this.dialogSubject.value.filter((d) => d !== dialog));
		};

		subject.subscribe({ complete: removeDialog, error: removeDialog });

		this.dialogSubject.next([...this.dialogSubject.value, dialog]);

		return subject.toPromise();
	}
}
