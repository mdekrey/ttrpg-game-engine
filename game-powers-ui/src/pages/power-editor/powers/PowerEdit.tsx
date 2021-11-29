import * as yup from 'yup';
import { PowerTextProfile } from 'api/models/PowerTextProfile';
import { Button } from 'components/button/Button';
import { TextboxField } from 'components/forms';
import { PowerTextBlock } from 'components/power';
import { PowerType } from 'components/power/Power';
import { useApi } from 'core/hooks/useApi';
import { useGameForm } from 'core/hooks/useGameForm';
import { useObservable } from 'core/hooks/useObservable';
import { initial, isLoading, Loadable, makeError, makeLoaded, makeLoading } from 'core/loadable/loadable';
import { map, startWith, switchAll, tap } from 'rxjs/operators';
import { SaveIcon, TrashIcon } from '@heroicons/react/solid';
import { ButtonRow } from 'components/ButtonRow';
import { Spinner } from 'components/spinner/spinner';
import { Subject } from 'rxjs';
import { RequestParams } from 'api/operations/setPowerFlavor';
import useConstant from 'use-constant';
import { useMemo } from 'react';
import { ObjectShape } from 'yup/lib/object';

type FlavorText = Record<string, string>;
// export type PowerTextInfo = {
// 	'Name': string;
// 	'Flavor Text': string;
// };

// export const powerTextInfoSchema: yup.SchemaOf<FlavorText> = yup.object({
// 	name: yup.string().required().label('Name'),
// 	flavorText: yup.string().required().label('Flavor Text'),
// });

export function PowerEdit({
	power,
	param,
	onRequestReload,
}: {
	power: PowerTextProfile;
	param: RequestParams;
	onRequestReload: () => void;
}) {
	const keys = Object.keys(power.flavor).sort().join(',');
	const schema = useMemo(() => {
		const result: ObjectShape = {};
		keys.split(',').forEach((key) => {
			result[key] = yup.string().required().label(key);
		});
		return yup.object(result) as yup.SchemaOf<FlavorText>;
	}, [keys]);

	const api = useApi();
	const submitSubject = useConstant(() => new Subject<FlavorText>());
	const deleteSubject = useConstant(() => new Subject<void>());
	const { powerUsage, attackType, name: originalName, flavorText: originalFlavor, ...text } = power.text;

	const { handleSubmit, ...form } = useGameForm<FlavorText>({
		defaultValues: power.flavor,
		schema,
	});

	const maybeSaving = useObservable<Loadable<boolean>>(
		() =>
			submitSubject.pipe(
				map((data) =>
					api.setPowerFlavor(param, data, 'application/json').pipe(
						map((response) => (response.statusCode === 200 ? makeLoaded(true) : makeError(response))),
						startWith(makeLoading()),
						tap((status) => status.type === 'loaded' && onRequestReload && onRequestReload())
					)
				),
				switchAll()
			),
		initial
	);

	const maybeDeleting = useObservable<Loadable<boolean>>(
		() =>
			deleteSubject.pipe(
				map(() =>
					api.replacePower(param).pipe(
						map((response) => (response.statusCode === 200 ? makeLoaded(true) : makeError(response))),
						startWith(makeLoading()),
						tap((status) => status.type === 'loaded' && onRequestReload && onRequestReload())
					)
				),
				switchAll()
			),
		initial
	);

	const disabled = isLoading(maybeSaving);

	if (maybeDeleting.type === 'loaded') return null;

	return (
		<div className="grid grid-cols-3 gap-4 print:grid-cols-2">
			<div className="col-span-2">
				<PowerTextBlock
					name={form.watch('Name')}
					flavorText={form.watch('Flavor Text')}
					{...text}
					powerUsage={powerUsage as PowerType}
					attackType={(attackType || null) as 'Personal' | 'Ranged' | 'Melee' | 'Close' | 'Area' | null}
				/>
			</div>
			<form
				onSubmit={disabled ? () => {} : handleSubmit((d) => submitSubject.next(d))}
				className="grid grid-cols-1 gap-3 self-start print:hidden">
				{keys.split(',').map((key) => (
					<TextboxField key={key} label={key} form={form} name={key} disabled={disabled} />
				))}
				<ButtonRow>
					<Button contents="icon" type="submit" disabled={disabled}>
						{isLoading(maybeSaving) ? <Spinner /> : <SaveIcon className="w-5 h-5" />}
					</Button>
					<Button contents="icon" look="cancel" disabled={disabled} onClick={() => deleteSubject.next()}>
						{isLoading(maybeDeleting) ? <Spinner /> : <TrashIcon className="w-5 h-5" />}
					</Button>
				</ButtonRow>
			</form>
		</div>
	);
}
