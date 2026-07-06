window.pokerEmoji = {
  getElementCenter: function (elementId) {
    const element = document.getElementById(elementId);
    if (!element) {
      return null;
    }

    const rect = element.getBoundingClientRect();
    const x = ((rect.left + rect.width / 2) / window.innerWidth) * 100;
    const y = ((rect.top + rect.height / 2) / window.innerHeight) * 100;

    return { x, y };
  }
};
