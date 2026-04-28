<script>
  import { onMount } from 'svelte'

  export let type = 'info' // success, error, warning, info
  export let message = ''
  export let onClose = null
  export let dismissible = true

  let visible = true

  const handleClose = () => {
    visible = false
    if (onClose) onClose()
  }

  onMount(() => {
    if (visible && dismissible) {
      const timer = setTimeout(() => {
        visible = false
      }, 5000)
      return () => clearTimeout(timer)
    }
  })
</script>

{#if visible && message}
  <div class="alert alert-{type}">
    <div class="flex gap-2">
      <div>
        {#if type === 'success'}
          <i class="fas fa-check-circle"></i>
        {:else if type === 'error'}
          <i class="fas fa-exclamation-circle"></i>
        {:else if type === 'warning'}
          <i class="fas fa-exclamation-triangle"></i>
        {:else}
          <i class="fas fa-info-circle"></i>
        {/if}
      </div>
      <div class="flex-1">
        {message}
      </div>
      {#if dismissible}
        <button class="alert-close" on:click={handleClose}>
          <i class="fas fa-times"></i>
        </button>
      {/if}
    </div>
  </div>
{/if}

<style>
  .alert-close {
    background: none;
    border: none;
    cursor: pointer;
    font-size: 1rem;
    opacity: 0.7;
    transition: opacity 0.2s;
  }

  .alert-close:hover {
    opacity: 1;
  }
</style>
