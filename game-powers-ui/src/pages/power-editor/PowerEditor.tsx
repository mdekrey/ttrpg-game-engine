import { PencilIcon, LockOpenIcon } from '@heroicons/react/solid';
import { ClassDetailsReadOnly } from 'api/models/ClassDetailsReadOnly';
import { EditableClassProfile } from 'api/models/EditableClassProfile';
import { Button } from 'components/button/Button';
import { ButtonRow } from 'components/ButtonRow';
import { Modal } from 'components/modal/modal';
import { ReaderLayout } from 'components/reader-layout';
import { useApi } from 'core/hooks/useApi';
import { useObservable } from 'core/hooks/useObservable';
import { initial, makeError, makeLoaded, makeLoading } from 'core/loadable/loadable';
import { LoadableComponent } from 'core/loadable/LoadableComponent';
import { groupBy } from 'lodash/fp';
import { ClassSurveyForm } from 'pages/class-survey/form/class-survey-form';
import { useState } from 'react';
import { BehaviorSubject, combineLatest } from 'rxjs';
import { delay, map, repeatWhen, switchAll, takeWhile } from 'rxjs/operators';
import useConstant from 'use-constant';
import { PowerSection } from './powers/PowerSection';

type ReasonCode = 'NotFound';

export function PowerEditor({ data: { classId } }: { data: { classId: string } }) {
	const [editingClass, setEditingClass] = useState(false);
	const [considerLock, setConsiderLock] = useState(false);
	const restartPolling = useConstant(() => new BehaviorSubject<void>(undefined));
	const api = useApi();
	const data = useObservable(
		(input$) =>
			combineLatest([input$, restartPolling]).pipe(
				map(([inputs]) => inputs),
				map(([id]) =>
					api.getClass({ params: { id } }).pipe(
						repeatWhen((completed) => completed.pipe(delay(1000))),
						takeWhile((response) => response.statusCode === 200 && response.data.state === 'In Progress', true),
						map((response) =>
							response.statusCode === 404
								? makeError<ReasonCode>('NotFound' as const)
								: response.data.state === 'In Progress'
								? makeLoading(response.data)
								: makeLoaded(response.data)
						)
					)
				),
				switchAll()
			),
		initial,
		[classId] as const
	);

	const updateClass = async (newData: EditableClassProfile) => {
		await api.updateClass({ body: newData, params: { id: classId } }).toPromise();
		setEditingClass(false);
		restartPolling.next();
	};

	const lockClass = async () => {
		await api.lockClass({ params: { id: classId } }).toPromise();
		window.location.href = `/class/${classId}`;
	};

	return (
		<div className="p-8">
			<LoadableComponent
				data={data}
				errorComponent={() => <>Not Found</>}
				loadedComponent={(loaded, isLoadingNext) => {
					const powers = toPowerTextGroups(loaded.original);
					return (
						<>
							<ReaderLayout>
								<h1 className="font-header font-bold mt-4 first:mt-0 text-theme text-2xl">
									{loaded.original.name}
									<Button className="ml-2" contents="icon" look="primary" onClick={() => setEditingClass(true)}>
										<PencilIcon className="w-5 h-5 inline-block" />
									</Button>
									<Button className="ml-2" contents="icon" look="primary" onClick={() => setConsiderLock(true)}>
										<LockOpenIcon className="w-5 h-5 inline-block" />
									</Button>
								</h1>
								{Object.keys(powers).map((header) => (
									<PowerSection
										header={header}
										key={header}
										powers={powers[header]}
										classId={classId}
										onRequestReload={() => restartPolling.next()}
									/>
								))}
							</ReaderLayout>
							{isLoadingNext ? <>Loading</> : null}
							<Modal show={editingClass} onClose={() => setEditingClass(false)} title="Alter Class" size="full">
								<ClassSurveyForm
									defaultValues={loaded.original}
									onSubmit={updateClass}
									onCancel={() => setEditingClass(false)}
								/>
							</Modal>
							<Modal show={considerLock} onClose={() => setConsiderLock(false)} title="Lock Class" size="medium">
								<form onSubmit={lockClass}>
									<p>Locking the class cannot be undone. Are you sure all your edits are complete?</p>
									<ButtonRow className="col-span-6">
										<Button type="submit">Lock</Button>
										<Button type="button" onClick={() => setConsiderLock(false)} look="cancel">
											Cancel
										</Button>
									</ButtonRow>
								</form>
							</Modal>
						</>
					);
				}}
				loadingComponent={<>Loading</>}
			/>
		</div>
	);

	function toPowerTextGroups(responseData: ClassDetailsReadOnly) {
		return groupBy(
			(block) => `${block.profile.level && `Level ${block.profile.level} `}${block.profile.usage} Powers`,
			responseData.powers
		);
	}
}
