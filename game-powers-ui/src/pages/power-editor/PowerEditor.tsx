import { PencilIcon, LockOpenIcon, TrashIcon } from '@heroicons/react/solid';
import { ClassDetailsReadOnly } from 'src/api/models/ClassDetailsReadOnly';
import { EditableClassDescriptor } from 'src/api/models/EditableClassDescriptor';
import { Button } from 'src/components/button/Button';
import { ButtonRow } from 'src/components/ButtonRow';
import { ClassDescription } from 'src/components/mdx/ClassDescription';
import { Modal } from 'src/components/modal/modal';
import { ReaderLayout } from 'src/components/reader-layout';
import { useApi } from 'src/core/hooks/useApi';
import { useObservable } from 'src/core/hooks/useObservable';
import { initial, isLoaded, makeError, makeLoaded, makeLoading } from 'src/core/loadable/loadable';
import { LoadableComponent } from 'src/core/loadable/LoadableComponent';
import { groupBy } from 'lodash/fp';
import { ClassSurveyForm } from 'src/pages/class-survey/form/class-survey-form';
import { useEffect, useState } from 'react';
import { BehaviorSubject, combineLatest } from 'rxjs';
import { delay, map, repeatWhen, switchAll, takeWhile } from 'rxjs/operators';
import useConstant from 'use-constant';
import { PowerSection } from './powers/PowerSection';

type ReasonCode = 'NotFound';

export function PowerEditor({ data: { classId } }: { data: { classId: string } }) {
	const [modalState, setModalState] = useState<'None' | 'Edit' | 'Lock' | 'Delete'>('None');
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

	useEffect(() => {
		if (isLoaded(data) && data.value.state === 'Read-Only') window.location.href = `/class/${classId}`;
	}, [data, classId]);

	const updateClass = async (newData: EditableClassDescriptor) => {
		await api.updateClass({ body: newData, params: { id: classId } }).toPromise();
		setModalState('None');
		restartPolling.next();
	};

	const lockClass = async () => {
		await api.lockClass({ params: { id: classId } }).toPromise();
		window.location.href = `/class/${classId}`;
	};

	const deleteClass = async () => {
		await api.deleteClass({ params: { id: classId } }).toPromise();
		window.location.href = `/`;
	};

	const closeModal = () => setModalState('None');
	const setEditingClass = () => setModalState('Edit');
	const setConsiderLock = () => setModalState('Lock');
	const setConsiderDelete = () => setModalState('Delete');

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
									<Button className="ml-2" contents="icon" look="primary" onClick={() => setEditingClass()}>
										<PencilIcon className="w-5 h-5 inline-block" />
									</Button>
									<Button className="ml-2" contents="icon" look="primary" onClick={() => setConsiderLock()}>
										<LockOpenIcon className="w-5 h-5 inline-block" />
									</Button>
									<Button className="ml-2" contents="icon" look="cancel" onClick={() => setConsiderDelete()}>
										<TrashIcon className="w-5 h-5 inline-block" />
									</Button>
								</h1>
								<ClassDescription mdx={loaded.original.description || ''} />
								{Object.keys(powers).map((header) => (
									<PowerSection
										classProfile={loaded.original}
										header={header}
										key={header}
										powers={powers[header]}
										classId={classId}
										onRequestReload={() => restartPolling.next()}
									/>
								))}
							</ReaderLayout>
							{isLoadingNext ? <>Loading</> : null}
							<Modal show={modalState === 'Edit'} onClose={closeModal} title="Alter Class" size="full">
								<ClassSurveyForm defaultValues={loaded.original} onSubmit={updateClass} onCancel={closeModal} />
							</Modal>
							<Modal show={modalState === 'Lock'} onClose={closeModal} title="Lock Class" size="medium">
								<form onSubmit={lockClass}>
									<p>Locking the class cannot be undone. Are you sure all your edits are complete?</p>
									<ButtonRow className="col-span-6">
										<Button type="submit">Lock</Button>
										<Button type="button" onClick={closeModal} look="cancel">
											Cancel
										</Button>
									</ButtonRow>
								</form>
							</Modal>
							<Modal show={modalState === 'Delete'} onClose={closeModal} title="Delete Class" size="medium">
								<form onSubmit={deleteClass}>
									<p>Deleting the class cannot be undone. Are you sure?</p>
									<ButtonRow className="col-span-6">
										<Button type="submit">Delete</Button>
										<Button type="button" onClick={closeModal} look="cancel">
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
		return groupBy((block) => `${block.level && `Level ${block.level} `}${block.usage} Powers`, responseData.powers);
	}
}
