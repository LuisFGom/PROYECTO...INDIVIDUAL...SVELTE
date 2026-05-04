<script>
  export let label = ''
  export let name = ''
  export let type = 'text'
  export let value = ''
  export let placeholder = ''
  export let required = false
  export let error = ''
  export let onChange = null
  export let onBlur = null
  export let disabled = false
  export let pattern = ''
  export let maxlength = null

  const handleChange = (e) => {
    value = e.target.value
    if (onChange) onChange(value)
  }

  const handleBlur = (e) => {
    if (onBlur) onBlur(e.target.value)
  }

  const handleInput = (e) => {
    value = e.target.value
    if (onChange) onChange(value)
  }
</script>

<div class="form-group">
  {#if label}
    <label class="label" for={name}>
      {label}
      {#if required}
        <span class="text-error">*</span>
      {/if}
    </label>
  {/if}
  {#if type === 'email'}
    <input
      class="input {error ? 'error' : ''}"
      type="email"
      {name}
      bind:value
      {placeholder}
      {disabled}
      {maxlength}
      pattern={pattern || '.*'}
      on:change={handleChange}
      on:blur={handleBlur}
      on:input={handleChange}
      on:keypress
      on:paste
      on:keydown
    />
  {:else if type === 'password'}
    <input
      class="input {error ? 'error' : ''}"
      type="password"
      {name}
      bind:value
      {placeholder}
      {disabled}
      {maxlength}
      pattern={pattern || '.*'}
      on:change={handleChange}
      on:blur={handleBlur}
      on:input={handleChange}
      on:keypress
      on:paste
      on:keydown
    />
  {:else if type === 'tel'}
    <input
      class="input {error ? 'error' : ''}"
      type="tel"
      {name}
      bind:value
      {placeholder}
      {disabled}
      {maxlength}
      pattern={pattern || '.*'}
      on:change={handleChange}
      on:blur={handleBlur}
      on:input={handleChange}
      on:keypress
      on:paste
      on:keydown
    />
  {:else if type === 'number'}
    <input
      class="input {error ? 'error' : ''}"
      type="number"
      {name}
      bind:value
      {placeholder}
      {disabled}
      {maxlength}
      pattern={pattern || '.*'}
      on:change={handleChange}
      on:blur={handleBlur}
      on:input={handleChange}
      on:keypress
      on:paste
      on:keydown
    />
  {:else}
    <input
      class="input {error ? 'error' : ''}"
      type="text"
      {name}
      bind:value
      {placeholder}
      {disabled}
      {maxlength}
      pattern={pattern || '.*'}
      on:change={handleChange}
      on:blur={handleBlur}
      on:input={handleChange}
      on:keypress
      on:paste
      on:keydown
    />
  {/if}
  {#if error}
    <div class="error-message">{error}</div>
  {/if}
</div>

<style>
  .text-error {
    color: #ef4444;
  }
</style>
