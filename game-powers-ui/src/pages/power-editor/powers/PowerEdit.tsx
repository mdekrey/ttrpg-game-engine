/* eslint-disable react/no-array-index-key */
import * as yup from 'yup';
import { PowerTextProfile } from 'api/models/PowerTextProfile';
import { Button } from 'components/button/Button';
import { SelectField, TextboxField } from 'components/forms';
import { PowerTextBlock } from 'components/power';
import { useApi } from 'core/hooks/useApi';
import { useGameForm } from 'core/hooks/useGameForm';
import { useObservable } from 'core/hooks/useObservable';
import { initial, isLoading, Loadable, makeError, makeLoaded, makeLoading } from 'core/loadable/loadable';
import { map, startWith, switchAll, tap } from 'rxjs/operators';
import { CheckIcon, PencilAltIcon, TrashIcon } from '@heroicons/react/solid';
import { ButtonRow } from 'components/ButtonRow';
import { Spinner } from 'components/spinner/spinner';
import { Subject } from 'rxjs';
import { RequestParams } from 'api/operations/setPowerFlavor';
import useConstant from 'use-constant';
import { useEffect, useMemo, useState } from 'react';
import { ObjectShape } from 'yup/lib/object';
import { powerTextBlockToProps } from 'components/power/PowerTextBlock';
import { ClassProfile } from 'api/models/ClassProfile';
import { Modal } from 'components/modal/modal';
import { HandcraftPower, SelectPowerResult } from './HandcraftPower';

type FlavorText = Record<string, string>;

export function PowerEdit({
	classProfile,
	power,
	param,
	onRequestReload,
}: {
	classProfile: ClassProfile;
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
	const { name: originalName, flavorText: originalFlavor, ...text } = power.text;

	const { handleSubmit, ...form } = useGameForm<FlavorText>({
		defaultValues: power.flavor,
		schema,
	});
	const { reset } = form;

	useEffect(() => {
		reset(power.flavor);
	}, [reset, power.flavor]);

	const submitSubject = useConstant(() => new Subject<FlavorText>());
	const maybeSaving = useObservable<Loadable<boolean>>(
		() =>
			submitSubject.pipe(
				map((data) =>
					api.setPowerFlavor({ params: param, body: data }).pipe(
						map((response) => (response.statusCode === 200 ? makeLoaded(true) : makeError(response))),
						startWith(makeLoading()),
						tap((status) => status.type === 'loaded' && onRequestReload && onRequestReload())
					)
				),
				switchAll()
			),
		initial
	);

	const deleteSubject = useConstant(() => new Subject<void>());
	const maybeDeleting = useObservable<Loadable<boolean>>(
		() =>
			deleteSubject.pipe(
				map(() =>
					api.replacePower({ params: param }).pipe(
						map((response) => (response.statusCode === 200 ? makeLoaded(true) : makeError(response))),
						startWith(makeLoading()),
						tap((status) => status.type === 'loaded' && onRequestReload && onRequestReload())
					)
				),
				switchAll()
			),
		initial
	);

	const [showHandCraftingModal, setShowHandCraftingModal] = useState(false);
	const [selectedToolIndex, setSelectedToolIndex] = useState<number>(0);
	const saveHandcraftedSubject = useConstant(() => new Subject<SelectPowerResult>());
	const maybeReplacing = useObservable<Loadable<boolean>>(
		() =>
			saveHandcraftedSubject.pipe(
				map((body) =>
					api.replacePowerWith({ params: param, mimeType: 'application/json', body }).pipe(
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
					{...powerTextBlockToProps({ ...text, name: form.watch('Name'), flavorText: form.watch('Flavor Text') })}
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
						{isLoading(maybeSaving) ? <Spinner /> : <CheckIcon />}
					</Button>
					<Button contents="icon" look="cancel" disabled={disabled} onClick={() => deleteSubject.next()}>
						{isLoading(maybeDeleting) ? <Spinner /> : <TrashIcon />}
					</Button>
					<Button contents="icon" disabled={disabled} onClick={() => setShowHandCraftingModal(true)}>
						{isLoading(maybeReplacing) ? <Spinner /> : <PencilAltIcon />}
					</Button>
				</ButtonRow>
			</form>

			<Modal
				show={showHandCraftingModal}
				onClose={() => setShowHandCraftingModal(false)}
				title="Handcraft Power"
				size="full">
				<SelectField
					className="my-4"
					label="Preview Powers"
					value={`${selectedToolIndex}`}
					onChange={({ currentTarget: { value } }) => {
						setSelectedToolIndex(Number(value));
					}}>
					{classProfile.tools.map((tool, toolIndex) => (
						<option value={toolIndex} key={toolIndex}>
							{`${tool.toolRange} ${tool.toolType}`}
						</option>
					))}
				</SelectField>

				<HandcraftPower
					key={selectedToolIndex}
					classProfile={classProfile}
					toolIndex={selectedToolIndex}
					powerProfileIndex={null}
					level={power.level!}
					usage={power.usage}
					onCancel={() => setShowHandCraftingModal(false)}
					onSelectPower={(body) => {
						saveHandcraftedSubject.next(body);
						setShowHandCraftingModal(false);
					}}
				/>
			</Modal>
		</div>
	);
}
