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
import { map, startWith, switchAll } from 'rxjs/operators';
import { SaveIcon } from '@heroicons/react/solid';
import { ButtonRow } from 'components/ButtonRow';
import { Spinner } from 'components/spinner/spinner';
import { useMemo } from 'react';
import { Subject } from 'rxjs';
import { RequestParams } from 'api/operations/setPowerFlavor';

export type PowerTextInfo = {
	name: string;
	flavorText: string;
};

export const powerTextInfoSchema: yup.SchemaOf<PowerTextInfo> = yup.object({
	name: yup.string().required().label('Name'),
	flavorText: yup.string().required().label('Flavor Text'),
});

export function PowerEdit({ power, param }: { power: PowerTextProfile; param: RequestParams }) {
	const api = useApi();
	const submitSubject = useMemo(() => new Subject<PowerTextInfo>(), []);
	const { powerUsage, attackType, name: originalName, flavorText: originalFlavor, ...text } = power.text;

	const { handleSubmit, ...form } = useGameForm<PowerTextInfo>({
		defaultValues: {
			name: originalName,
			flavorText: originalFlavor || '',
		},
		schema: powerTextInfoSchema,
	});

	const maybe = useObservable<Loadable<boolean>>(
		() =>
			submitSubject.pipe(
				map((data) =>
					api.setPowerFlavor(param, data, 'application/json').pipe(
						map((response) => (response.statusCode === 200 ? makeLoaded(true) : makeError(response))),
						startWith(makeLoading())
					)
				),
				switchAll()
			),
		initial
	);

	const disabled = isLoading(maybe);

	return (
		<div className="grid grid-cols-3 gap-4">
			<PowerTextBlock
				className="col-span-2"
				name={form.watch('name')}
				flavorText={form.watch('flavorText')}
				{...text}
				powerUsage={powerUsage as PowerType}
				attackType={(attackType || null) as 'Personal' | 'Ranged' | 'Melee' | 'Close' | 'Area' | null}
			/>
			<form
				onSubmit={disabled ? () => {} : handleSubmit((d) => submitSubject.next(d))}
				className="grid grid-cols-1 gap-3 self-start">
				<TextboxField label="Name" form={form} name="name" disabled={disabled} />
				<TextboxField label="Flavor Text" form={form} name="flavorText" disabled={disabled} />
				<ButtonRow>
					<Button contents="icon" type="submit" disabled={disabled}>
						{disabled ? <Spinner /> : <SaveIcon className="w-5 h-5" />}
					</Button>
				</ButtonRow>
			</form>
		</div>
	);
}
