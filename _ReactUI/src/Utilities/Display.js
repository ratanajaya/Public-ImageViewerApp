import { useMediaQuery } from 'react-responsive'

const DesktopOnly = ({ children }) => {
  const isDesktop = useMediaQuery({ minWidth: 801 })
  return isDesktop ? children : null
}
const MobileOnly = ({ children }) => {
  const isMobile = useMediaQuery({ maxWidth: 800 })
  return isMobile ? children : null
}

export { DesktopOnly, MobileOnly };