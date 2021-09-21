export function Spinner() {
	return (
		<div className=" flex justify-center items-center">
			<div className="motion-safe:animate-none animate-spin rounded-full h-em w-em border-b-2 border-currentcolor" />
		</div>
	);
}
