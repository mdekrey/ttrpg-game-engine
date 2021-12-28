import { sortBy } from 'lodash/fp';
import { ClassProfile } from 'api/models/ClassProfile';
import { ReaderLayout } from 'components/reader-layout';

type ClassEntry = {
	name: string;
	classProfile: ClassProfile;
};

export function ClassList({ data: { classes } }: { data: { classes: Record<string, ClassEntry> } }) {
	const eachClass = sortBy(
		(v) => v.name,
		Object.keys(classes).map((key) => ({ id: key, ...classes[key] }))
	);
	return (
		<ReaderLayout>
			<ul className="list-disc ml-6 theme-4e-list">
				{eachClass.map(({ id, name }) => (
					<li key={id} className="my-1">
						<a href={`/class/${id}`} className="underline text-theme">
							{name}
						</a>
					</li>
				))}
			</ul>
		</ReaderLayout>
	);
}
