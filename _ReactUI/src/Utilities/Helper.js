
export function firstLetterLowerCase(string) {
  return string.charAt(0).toLowerCase() + string.slice(1);
}

export function apiErrorHandler(error) {
  if (typeof error.response !== undefined) {
    alert(JSON.stringify(error.response, null, "\t"))
  }
  else {
    alert(JSON.stringify(error, null, "\t"));
  }
}

export function getPercent100(value, maxValue) {
  let fraction = (value / maxValue) * 100;
  let result = fraction <= 100 ? fraction : 100;
  return result;
}

export function contains(target, pattern) {
  var value = 0;
  pattern.forEach(function (word) {
    value = value + target.includes(word);
  });
  return (value === 1);
}

export function clamp(number, min, max) {
  return Math.max(min, Math.min(number, max));
}