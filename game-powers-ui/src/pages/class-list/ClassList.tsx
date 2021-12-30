import { sortBy } from 'lodash/fp';
import { ClassDescriptor } from 'api/models/ClassDescriptor';
import { ReaderLayout } from 'components/reader-layout';
import { LockClosedIcon, PencilIcon } from '@heroicons/react/solid';

export function ClassList({ data: { classes } }: { data: { classes: Record<string, ClassDescriptor> } }) {
	const eachClass = sortBy(
		(v) => v.name,
		Object.keys(classes).map((key) => ({ id: key, ...classes[key] }))
	);
	return (
		<ReaderLayout>
			<ul className="list-disc ml-6 theme-4e-list">
				{eachClass.map(({ id, name, state }) => (
					<li key={id} className="my-1">
						<a href={`/class/${id}`} className="underline text-theme">
							{name}
						</a>
						{state === 'Read-Only' ? (
							<LockClosedIcon className="w-5 h-5 inline-block" />
						) : (
							<a href={`/class/edit/${id}`} className="underline text-theme">
								<PencilIcon className="w-5 h-5 inline-block" />
							</a>
						)}
					</li>
				))}
			</ul>
		</ReaderLayout>
	);
}
