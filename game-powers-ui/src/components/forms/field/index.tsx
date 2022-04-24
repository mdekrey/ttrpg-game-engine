export * from './field.context';
export * from './field.component';

/*
TODO: thie goal would be to get somewhere like:

	<Field className="col-span-6 sm:col-span-3" form={form} name="name">
		<Field.Label />
		<Field.Textbox />
		<Field.ValidationMessages />
	</Field>

- Label would come from the validation schema, etc.
- Type checking for the downstream controls would be good, so maybe something like:

		<Field className="col-span-6 sm:col-span-3" form={form} name="name">
			({ Label, Textbox, ValidationMessages }) => (
				<Label />
				<Textbox />
				<ValidationMessages />
			)
		</Field>

	I don't like the syntax... but the values passed to the function could be memoized and type-cast.

*/
